using System.ComponentModel;

namespace NetworkSpeedWidget.Models;

public class ProcessNetworkInfo : INotifyPropertyChanged
{
    private string processName = string.Empty;
    private int processId;
    private double downloadSpeed;
    private double uploadSpeed;
    private long totalDownloaded;
    private long totalUploaded;

    public string ProcessName
    {
        get => processName;
        set
        {
            processName = value;
            OnPropertyChanged(nameof(ProcessName));
        }
    }

    public int ProcessId
    {
        get => processId;
        set
        {
            processId = value;
            OnPropertyChanged(nameof(ProcessId));
        }
    }

    public double DownloadSpeed
    {
        get => downloadSpeed;
        set
        {
            downloadSpeed = value;
            OnPropertyChanged(nameof(DownloadSpeed));
            OnPropertyChanged(nameof(DownloadSpeedFormatted));
        }
    }

    public double UploadSpeed
    {
        get => uploadSpeed;
        set
        {
            uploadSpeed = value;
            OnPropertyChanged(nameof(UploadSpeed));
            OnPropertyChanged(nameof(UploadSpeedFormatted));
        }
    }

    public long TotalDownloaded
    {
        get => totalDownloaded;
        set
        {
            totalDownloaded = value;
            OnPropertyChanged(nameof(TotalDownloaded));
        }
    }

    public long TotalUploaded
    {
        get => totalUploaded;
        set
        {
            totalUploaded = value;
            OnPropertyChanged(nameof(TotalUploaded));
        }
    }

    public string DownloadSpeedFormatted => NetworkMonitor.FormatSpeed(DownloadSpeed);
    public string UploadSpeedFormatted => NetworkMonitor.FormatSpeed(UploadSpeed);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
