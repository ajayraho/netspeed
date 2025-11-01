using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using NetworkSpeedWidget.Models;

namespace NetworkSpeedWidget.Services;

public class ConnectionMonitor : IDisposable
{
    private readonly ObservableCollection<ProcessConnectionInfo> processConnections;
    private bool disposed = false;

    public ObservableCollection<ProcessConnectionInfo> ProcessConnections => processConnections;

    public ConnectionMonitor()
    {
        processConnections = new ObservableCollection<ProcessConnectionInfo>();
    }

    public void Update()
    {
        try
        {
            var connectionsByProcess = new Dictionary<int, List<ConnectionInfo>>();
            var processNames = new Dictionary<int, string>();

            // Get TCP connections
            var tcpConnections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            foreach (var conn in tcpConnections)
            {
                try
                {
                    // Use GetExtendedTcpTable to get process ID
                    var pid = GetProcessIdFromConnection(conn);
                    if (pid > 0)
                    {
                        if (!connectionsByProcess.ContainsKey(pid))
                        {
                            connectionsByProcess[pid] = new List<ConnectionInfo>();
                            try
                            {
                                var process = Process.GetProcessById(pid);
                                processNames[pid] = process.ProcessName;
                            }
                            catch
                            {
                                processNames[pid] = $"PID {pid}";
                            }
                        }

                        var connInfo = new ConnectionInfo
                        {
                            RemoteAddress = conn.RemoteEndPoint.Address.ToString(),
                            RemotePort = conn.RemoteEndPoint.Port,
                            State = conn.State.ToString(),
                            Protocol = "TCP"
                        };

                        // Try to resolve hostname (async, non-blocking)
                        Task.Run(() => ResolveHostname(connInfo));

                        connectionsByProcess[pid].Add(connInfo);
                    }
                }
                catch { }
            }

            // Update the observable collection
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                // Remove processes that no longer exist
                var toRemove = processConnections
                    .Where(p => !connectionsByProcess.ContainsKey(p.ProcessId))
                    .ToList();
                foreach (var item in toRemove)
                {
                    processConnections.Remove(item);
                }

                // Update or add processes
                foreach (var kvp in connectionsByProcess)
                {
                    var existing = processConnections.FirstOrDefault(p => p.ProcessId == kvp.Key);
                    if (existing != null)
                    {
                        // Update existing
                        existing.Connections = kvp.Value;
                        existing.ConnectionCount = kvp.Value.Count;
                    }
                    else
                    {
                        // Add new
                        processConnections.Add(new ProcessConnectionInfo
                        {
                            ProcessId = kvp.Key,
                            ProcessName = processNames[kvp.Key],
                            Connections = kvp.Value,
                            ConnectionCount = kvp.Value.Count
                        });
                    }
                }

                // Sort by connection count descending
                var sorted = processConnections.OrderByDescending(p => p.ConnectionCount).ToList();
                processConnections.Clear();
                foreach (var item in sorted)
                {
                    processConnections.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating connections: {ex.Message}");
        }
    }

    private int GetProcessIdFromConnection(TcpConnectionInformation conn)
    {
        try
        {
            // Get TCP table with process IDs
            var bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            
            var buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                if (GetExtendedTcpTable(buffer, ref bufferSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
                {
                    var table = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffer, typeof(MIB_TCPTABLE_OWNER_PID))!;
                    var rowPtr = (IntPtr)((long)buffer + Marshal.SizeOf(table.dwNumEntries));

                    for (int i = 0; i < table.dwNumEntries; i++)
                    {
                        var row = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID))!;
                        
                        var localPort = (ushort)((row.localPort >> 8) | ((row.localPort << 8) & 0xFF00));
                        var remotePort = (ushort)((row.remotePort >> 8) | ((row.remotePort << 8) & 0xFF00));

                        if (localPort == conn.LocalEndPoint.Port && 
                            remotePort == conn.RemoteEndPoint.Port &&
                            new IPAddress(row.remoteAddr).Equals(conn.RemoteEndPoint.Address))
                        {
                            return row.owningPid;
                        }

                        rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID)));
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch { }

        return 0;
    }

    private async void ResolveHostname(ConnectionInfo connInfo)
    {
        try
        {
            // Skip private/local addresses
            if (connInfo.RemoteAddress.StartsWith("127.") || 
                connInfo.RemoteAddress.StartsWith("192.168.") ||
                connInfo.RemoteAddress.StartsWith("10.") ||
                connInfo.RemoteAddress.StartsWith("172."))
            {
                return;
            }

            var hostEntry = await Dns.GetHostEntryAsync(connInfo.RemoteAddress);
            if (hostEntry != null && !string.IsNullOrEmpty(hostEntry.HostName))
            {
                connInfo.RemoteHost = hostEntry.HostName;
            }
        }
        catch
        {
            // DNS resolution failed, keep IP only
        }
    }

    #region P/Invoke

    private const int AF_INET = 2;

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetExtendedTcpTable(
        IntPtr pTcpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        TCP_TABLE_CLASS tblClass,
        int reserved);

    private enum TCP_TABLE_CLASS
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public int owningPid;
    }

    #endregion

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
        }
    }
}
