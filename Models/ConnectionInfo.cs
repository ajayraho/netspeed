using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetworkSpeedWidget.Models;

public class ConnectionInfo : INotifyPropertyChanged
{
    private string remoteAddress = string.Empty;
    private string remoteHost = string.Empty;
    private int remotePort;
    private string state = string.Empty;
    private string protocol = string.Empty;

    public string RemoteAddress
    {
        get => remoteAddress;
        set { remoteAddress = value; OnPropertyChanged(); }
    }

    public string RemoteHost
    {
        get => remoteHost;
        set { remoteHost = value; OnPropertyChanged(); }
    }

    public int RemotePort
    {
        get => remotePort;
        set { remotePort = value; OnPropertyChanged(); }
    }

    public string State
    {
        get => state;
        set { state = value; OnPropertyChanged(); }
    }

    public string Protocol
    {
        get => protocol;
        set { protocol = value; OnPropertyChanged(); }
    }

    public string DisplayText => string.IsNullOrEmpty(RemoteHost) 
        ? $"{RemoteAddress}:{RemotePort}" 
        : $"{RemoteHost} ({RemoteAddress}:{RemotePort})";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ProcessConnectionInfo : INotifyPropertyChanged
{
    private string processName = string.Empty;
    private int processId;
    private int connectionCount;
    private bool isExpanded = false;

    public string ProcessName
    {
        get => processName;
        set { processName = value; OnPropertyChanged(); }
    }

    public int ProcessId
    {
        get => processId;
        set { processId = value; OnPropertyChanged(); }
    }

    public int ConnectionCount
    {
        get => connectionCount;
        set { connectionCount = value; OnPropertyChanged(); }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set { isExpanded = value; OnPropertyChanged(); }
    }

    public List<ConnectionInfo> Connections { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }
}
