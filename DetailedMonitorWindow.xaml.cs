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
    private ICollectionView processGroupsView;

    public DetailedMonitorWindow()
    {
        InitializeComponent();
        
        processMonitor = new ProcessMonitor();
        
        // Use CollectionViewSource for sorting
        processGroupsView = CollectionViewSource.GetDefaultView(processMonitor.ProcessGroups);
        ProcessTreeView.ItemsSource = processGroupsView;

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

        // Clear existing sort descriptions
        processGroupsView.SortDescriptions.Clear();

        // Add new sort description
        var direction = sortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        
        switch (column)
        {
            case "name":
                processGroupsView.SortDescriptions.Add(new SortDescription("ProcessName", direction));
                break;
            case "download":
                processGroupsView.SortDescriptions.Add(new SortDescription("TotalDownloadSpeed", direction));
                break;
            case "upload":
                processGroupsView.SortDescriptions.Add(new SortDescription("TotalUploadSpeed", direction));
                break;
        }

        // Update sort icons
        GlobalDownloadSortIcon.Text = column == "download" ? (sortDescending ? "↓" : "↑") : "";
        GlobalUploadSortIcon.Text = column == "upload" ? (sortDescending ? "↓" : "↑") : "";
        
        // Refresh the view
        processGroupsView.Refresh();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        updateTimer.Stop();
        processMonitor.Dispose();
    }
}
