using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace NetworkSpeedWidget.Models;

public class ProcessGroupInfo : INotifyPropertyChanged
{
    private string processName = string.Empty;
    private ImageSource? icon;
    private double totalDownloadSpeed;
    private double totalUploadSpeed;
    private bool isExpanded = false; // Default collapsed
    private ObservableCollection<ProcessNetworkInfo> instances;
    private string downloadSortDirection = ""; // "", "↑", "↓"
    private string uploadSortDirection = "";

    public ProcessGroupInfo()
    {
        instances = new ObservableCollection<ProcessNetworkInfo>();
    }

    public string ProcessName
    {
        get => processName;
        set
        {
            processName = value;
            OnPropertyChanged(nameof(ProcessName));
        }
    }

    public ImageSource? Icon
    {
        get => icon;
        set
        {
            icon = value;
            OnPropertyChanged(nameof(Icon));
        }
    }

    public double TotalDownloadSpeed
    {
        get => totalDownloadSpeed;
        set
        {
            totalDownloadSpeed = value;
            OnPropertyChanged(nameof(TotalDownloadSpeed));
            OnPropertyChanged(nameof(TotalDownloadSpeedFormatted));
        }
    }

    public double TotalUploadSpeed
    {
        get => totalUploadSpeed;
        set
        {
            totalUploadSpeed = value;
            OnPropertyChanged(nameof(TotalUploadSpeed));
            OnPropertyChanged(nameof(TotalUploadSpeedFormatted));
        }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            isExpanded = value;
            OnPropertyChanged(nameof(IsExpanded));
        }
    }

    public ObservableCollection<ProcessNetworkInfo> Instances
    {
        get => instances;
        set
        {
            instances = value;
            OnPropertyChanged(nameof(Instances));
            OnPropertyChanged(nameof(InstanceCount));
        }
    }

    public int InstanceCount => instances.Count;

    public string TotalDownloadSpeedFormatted => NetworkMonitor.FormatSpeed(TotalDownloadSpeed);
    public string TotalUploadSpeedFormatted => NetworkMonitor.FormatSpeed(TotalUploadSpeed);

    public string DownloadSortIcon
    {
        get => downloadSortDirection;
        set
        {
            downloadSortDirection = value;
            OnPropertyChanged(nameof(DownloadSortIcon));
        }
    }

    public string UploadSortIcon
    {
        get => uploadSortDirection;
        set
        {
            uploadSortDirection = value;
            OnPropertyChanged(nameof(UploadSortIcon));
        }
    }

    public void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }

    public void SortByDownload()
    {
        var sorted = DownloadSortIcon == "↓" 
            ? instances.OrderBy(i => i.DownloadSpeed).ToList()
            : instances.OrderByDescending(i => i.DownloadSpeed).ToList();
        
        instances.Clear();
        foreach (var item in sorted)
        {
            instances.Add(item);
        }

        DownloadSortIcon = DownloadSortIcon == "↓" ? "↑" : "↓";
        UploadSortIcon = "";
    }

    public void SortByUpload()
    {
        var sorted = UploadSortIcon == "↓"
            ? instances.OrderBy(i => i.UploadSpeed).ToList()
            : instances.OrderByDescending(i => i.UploadSpeed).ToList();
        
        instances.Clear();
        foreach (var item in sorted)
        {
            instances.Add(item);
        }

        UploadSortIcon = UploadSortIcon == "↓" ? "↑" : "↓";
        DownloadSortIcon = "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateTotals()
    {
        TotalDownloadSpeed = instances.Sum(i => i.DownloadSpeed);
        TotalUploadSpeed = instances.Sum(i => i.UploadSpeed);
        OnPropertyChanged(nameof(InstanceCount));
    }
}
