using System.Diagnostics;
using System.Net.NetworkInformation;

namespace NetworkSpeedWidget;

public class NetworkMonitor : IDisposable
{
    private NetworkInterface? activeInterface;
    private long lastBytesReceived = 0;
    private long lastBytesSent = 0;
    private DateTime lastUpdateTime;
    private bool disposed = false;

    public double DownloadSpeed { get; private set; }
    public double UploadSpeed { get; private set; }

    public NetworkMonitor()
    {
        activeInterface = GetActiveNetworkInterface();
        lastUpdateTime = DateTime.Now;
        
        // Initialize baseline
        if (activeInterface != null)
        {
            var stats = activeInterface.GetIPv4Statistics();
            lastBytesReceived = stats.BytesReceived;
            lastBytesSent = stats.BytesSent;
        }
    }

    private NetworkInterface? GetActiveNetworkInterface()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                            ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .ToList();

            // Prefer Ethernet or Wireless interfaces
            var preferred = interfaces.FirstOrDefault(ni =>
                ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);

            return preferred ?? interfaces.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting network interface: {ex.Message}");
            return null;
        }
    }

    public void Update()
    {
        try
        {
            if (activeInterface == null)
            {
                DownloadSpeed = 0;
                UploadSpeed = 0;
                return;
            }

            var currentTime = DateTime.Now;
            var timeDiff = (currentTime - lastUpdateTime).TotalSeconds;

            if (timeDiff < 0.1) // Avoid division by very small numbers
                return;

            var stats = activeInterface.GetIPv4Statistics();
            var currentBytesReceived = stats.BytesReceived;
            var currentBytesSent = stats.BytesSent;

            // Calculate speed (bytes per second)
            DownloadSpeed = (currentBytesReceived - lastBytesReceived) / timeDiff;
            UploadSpeed = (currentBytesSent - lastBytesSent) / timeDiff;

            // Update last values
            lastBytesReceived = currentBytesReceived;
            lastBytesSent = currentBytesSent;
            lastUpdateTime = currentTime;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating speeds: {ex.Message}");
            DownloadSpeed = 0;
            UploadSpeed = 0;
        }
    }

    public static string FormatSpeed(double bytesPerSecond)
    {
        string[] sizes = { "B/s", "KB/s", "MB/s", "GB/s" };
        int order = 0;
        double speed = bytesPerSecond;

        while (speed >= 1024 && order < sizes.Length - 1)
        {
            order++;
            speed /= 1024;
        }

        return $"{speed:0.##} {sizes[order]}";
    }

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
