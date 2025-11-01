using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;
using NetworkSpeedWidget.Services;
using NetworkSpeedWidget.Models;

namespace NetworkSpeedWidget;

public partial class DetailedMonitorWindow : Window
{
    private readonly ProcessMonitor processMonitor;
    private readonly DispatcherTimer updateTimer;
    private bool sortDescending = true;
    private string currentSortColumn = "";

    public DetailedMonitorWindow()
    {
        InitializeComponent();
        
        processMonitor = new ProcessMonitor();
        ProcessTreeView.ItemsSource = processMonitor.ProcessGroups;

        // Update every 1 second
        updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        updateTimer.Tick += UpdateTimer_Tick;
        updateTimer.Start();

        // Initial update on background thread
        Task.Run(() => UpdateDataAsync());
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        Task.Run(() => UpdateDataAsync());
    }

    private async Task UpdateDataAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                processMonitor.Update();
                
                Dispatcher.Invoke(() =>
                {
                    var totalProcesses = processMonitor.ProcessGroups.Sum(g => g.InstanceCount);
                    ProcessCountText.Text = $"({processMonitor.ProcessGroups.Count} apps, {totalProcesses} processes)";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating: {ex.Message}");
            }
        });
    }

    private void ExpanderArrow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ProcessGroupInfo group)
        {
            group.ToggleExpanded();
            e.Handled = true;
        }
    }

    private void SortByDownload_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ProcessGroupInfo group)
        {
            group.SortByDownload();
            e.Handled = true;
        }
    }

    private void SortByUpload_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ProcessGroupInfo group)
        {
            group.SortByUpload();
            e.Handled = true;
        }
    }

    private void SortByName_MouseDown(object sender, MouseButtonEventArgs e)
    {
        SortGroups("name");
    }

    private void SortAllByDownload_MouseDown(object sender, MouseButtonEventArgs e)
    {
        SortGroups("download");
    }

    private void SortAllByUpload_MouseDown(object sender, MouseButtonEventArgs e)
    {
        SortGroups("upload");
    }

    private void SortGroups(string column)
    {
        if (currentSortColumn == column)
        {
            sortDescending = !sortDescending;
        }
        else
        {
            sortDescending = true;
            currentSortColumn = column;
        }

        var sorted = column switch
        {
            "name" => sortDescending 
                ? processMonitor.ProcessGroups.OrderByDescending(g => g.ProcessName).ToList()
                : processMonitor.ProcessGroups.OrderBy(g => g.ProcessName).ToList(),
            "download" => sortDescending
                ? processMonitor.ProcessGroups.OrderByDescending(g => g.TotalDownloadSpeed).ToList()
                : processMonitor.ProcessGroups.OrderBy(g => g.TotalDownloadSpeed).ToList(),
            "upload" => sortDescending
                ? processMonitor.ProcessGroups.OrderByDescending(g => g.TotalUploadSpeed).ToList()
                : processMonitor.ProcessGroups.OrderBy(g => g.TotalUploadSpeed).ToList(),
            _ => processMonitor.ProcessGroups.ToList()
        };

        processMonitor.ProcessGroups.Clear();
        foreach (var group in sorted)
        {
            processMonitor.ProcessGroups.Add(group);
        }

        // Update sort icons
        GlobalDownloadSortIcon.Text = column == "download" ? (sortDescending ? "↓" : "↑") : "";
        GlobalUploadSortIcon.Text = column == "upload" ? (sortDescending ? "↓" : "↑") : "";
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        updateTimer.Stop();
        processMonitor.Dispose();
    }
}
