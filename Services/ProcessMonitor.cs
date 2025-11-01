using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using NetworkSpeedWidget.Models;
using NetworkSpeedWidget.Utils;

namespace NetworkSpeedWidget.Services;

public class ProcessMonitor : IDisposable
{
    private readonly ObservableCollection<ProcessGroupInfo> processGroups;
    private readonly Dictionary<int, ProcessNetworkStats> lastStats;
    private bool disposed = false;

    private class ProcessNetworkStats
    {
        public long ReadBytes { get; set; }
        public long WriteBytes { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public ObservableCollection<ProcessGroupInfo> ProcessGroups => processGroups;

    public ProcessMonitor()
    {
        processGroups = new ObservableCollection<ProcessGroupInfo>();
        lastStats = new Dictionary<int, ProcessNetworkStats>();
    }

    public void Update()
    {
        try
        {
            var processes = Process.GetProcesses()
                .Where(p => p.Id != 0 && !string.IsNullOrEmpty(p.ProcessName))
                .ToList();

            var currentTime = DateTime.Now;
            var processDataByName = new Dictionary<string, List<ProcessNetworkInfo>>();
            var currentPids = new HashSet<int>();

            foreach (var process in processes)
            {
                try
                {
                    var processName = process.ProcessName;
                    currentPids.Add(process.Id);

                    if (!processDataByName.ContainsKey(processName))
                    {
                        processDataByName[processName] = new List<ProcessNetworkInfo>();
                    }

                    // Get I/O statistics
                    long readBytes = 0;
                    long writeBytes = 0;
                    double downloadSpeed = 0;
                    double uploadSpeed = 0;

                    try
                    {
                        // Try to get I/O counters (this will fail for some system processes)
                        var ioCounters = GetProcessIOCounters(process.Handle);
                        if (ioCounters.HasValue)
                        {
                            readBytes = (long)ioCounters.Value.ReadTransferCount;
                            writeBytes = (long)ioCounters.Value.WriteTransferCount;

                            // Calculate speed if we have previous stats
                            if (lastStats.TryGetValue(process.Id, out var prevStats))
                            {
                                var timeDiff = (currentTime - prevStats.LastUpdate).TotalSeconds;
                                if (timeDiff > 0.1)
                                {
                                    downloadSpeed = (readBytes - prevStats.ReadBytes) / timeDiff;
                                    uploadSpeed = (writeBytes - prevStats.WriteBytes) / timeDiff;
                                    
                                    // Apply network factor heuristic
                                    var factor = IsLikelyNetworkProcess(processName) ? 0.3 : 0.1;
                                    downloadSpeed *= factor;
                                    uploadSpeed *= factor;
                                }
                            }

                            // Update stats
                            lastStats[process.Id] = new ProcessNetworkStats
                            {
                                ReadBytes = readBytes,
                                WriteBytes = writeBytes,
                                LastUpdate = currentTime
                            };
                        }
                    }
                    catch
                    {
                        // Can't access this process, skip
                    }

                    var processInfo = new ProcessNetworkInfo
                    {
                        ProcessId = process.Id,
                        ProcessName = processName,
                        DownloadSpeed = downloadSpeed,
                        UploadSpeed = uploadSpeed
                    };

                    processDataByName[processName].Add(processInfo);
                }
                catch
                {
                    // Skip processes we can't access
                }
                finally
                {
                    process.Dispose();
                }
            }

            // Clean up old stats
            var oldPids = lastStats.Keys.Where(pid => !currentPids.Contains(pid)).ToList();
            foreach (var pid in oldPids)
            {
                lastStats.Remove(pid);
            }

            // Update UI - UPDATE existing groups instead of clearing
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Update existing groups or create new ones
                foreach (var kvp in processDataByName.Where(k => k.Value.Count > 0))
                {
                    var groupName = kvp.Key;
                    var instances = kvp.Value;

                    // Find existing group
                    var group = processGroups.FirstOrDefault(g => g.ProcessName == groupName);

                    if (group == null)
                    {
                        // Create new group
                        group = new ProcessGroupInfo
                        {
                            ProcessName = groupName,
                            Icon = IconExtractor.GetProcessIcon(groupName, instances[0].ProcessId)
                        };
                        processGroups.Add(group);
                    }

                    // Update instances (preserve order, just update data)
                    group.Instances.Clear();
                    foreach (var instance in instances)
                    {
                        group.Instances.Add(instance);
                    }

                    group.UpdateTotals();
                }

                // Remove groups that no longer exist
                var groupsToRemove = processGroups
                    .Where(g => !processDataByName.ContainsKey(g.ProcessName))
                    .ToList();

                foreach (var group in groupsToRemove)
                {
                    processGroups.Remove(group);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating process monitor: {ex.Message}");
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetProcessIoCounters(IntPtr hProcess, out IO_COUNTERS lpIoCounters);

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    private IO_COUNTERS? GetProcessIOCounters(IntPtr handle)
    {
        try
        {
            if (GetProcessIoCounters(handle, out var counters))
            {
                return counters;
            }
        }
        catch { }
        return null;
    }

    private bool IsLikelyNetworkProcess(string processName)
    {
        var networkProcesses = new[]
        {
            "chrome", "firefox", "msedge", "opera", "brave", "iexplore",
            "discord", "teams", "slack", "zoom", "skype",
            "spotify", "steam", "epicgameslauncher", "origin",
            "qbittorrent", "utorrent", "transmission", "bittorrent",
            "dropbox", "onedrive", "googledrivesync", "backup",
            "outlook", "thunderbird", "mailbird",
            "vscodium", "code", "devenv", "rider",
            "node", "python", "java", "dotnet"
        };

        return networkProcesses.Any(np => 
            processName.ToLower().Contains(np.ToLower()));
    }

    public void Dispose()
    {
        if (!disposed)
        {
            processGroups.Clear();
            lastStats.Clear();
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
