using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Diagnostics;
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
    private bool isFirstUpdate = true;

    public DetailedMonitorWindow()
    {
        InitializeComponent();
        
        processMonitor = new ProcessMonitor();
        
        // Use CollectionViewSource for sorting
        processGroupsView = CollectionViewSource.GetDefaultView(processMonitor.ProcessGroups);
        ProcessTreeView.ItemsSource = processGroupsView;

        // Set default sort to download speed descending
        currentSortColumn = "download";
        sortDescending = true;
        processGroupsView.SortDescriptions.Add(new SortDescription("TotalDownloadSpeed", ListSortDirection.Descending));
        GlobalDownloadSortIcon.Text = "↓";

        // Subscribe to blocked processes changes
        NetworkBlocker.BlockedProcessesChanged += OnBlockedProcessesChanged;

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

    private void OnBlockedProcessesChanged()
    {
        // Refresh the view when blocked processes change
        Dispatcher.Invoke(() =>
        {
            processGroupsView?.Refresh();
        });
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
                    
                    // On first update, ensure the sort is applied
                    if (isFirstUpdate)
                    {
                        isFirstUpdate = false;
                        processGroupsView?.Refresh();
                    }
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

    private void OpenBlockedProcesses_Click(object sender, RoutedEventArgs e)
    {
        var blockedWindow = new BlockedProcessesWindow
        {
            Owner = this
        };
        blockedWindow.ShowDialog();
    }

    private void EndTask_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ProcessNetworkInfo processInfo)
        {
            var result = CustomDialog.Show(
                "End Task Confirmation",
                $"Are you sure you want to end task for:\n\n{processInfo.ProcessName} (PID: {processInfo.ProcessId})?",
                "❌");

            if (result)
            {
                try
                {
                    var process = Process.GetProcessById(processInfo.ProcessId);
                    process.Kill();
                    process.WaitForExit(3000);
                    
                    CustomDialog.ShowMessage(
                        "Success",
                        $"Successfully ended task: {processInfo.ProcessName}",
                        "✅");
                }
                catch (Exception ex)
                {
                    CustomDialog.ShowMessage(
                        "Error",
                        $"Failed to end task:\n\n{ex.Message}",
                        "⚠️");
                }
            }
        }
    }

    private void EndTaskGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ProcessGroupInfo groupInfo)
        {
            var result = CustomDialog.Show(
                "End All Tasks Confirmation",
                $"Are you sure you want to end ALL {groupInfo.InstanceCount} instances of:\n\n{groupInfo.ProcessName}?",
                "❌");

            if (result)
            {
                var killed = 0;
                var failed = 0;

                foreach (var instance in groupInfo.Instances.ToList())
                {
                    try
                    {
                        var process = Process.GetProcessById(instance.ProcessId);
                        process.Kill();
                        killed++;
                    }
                    catch
                    {
                        failed++;
                    }
                }

                CustomDialog.ShowMessage(
                    "Task Complete",
                    $"Ended {killed} process(es)\n{(failed > 0 ? $"Failed: {failed}" : "")}",
                    "✅");
            }
        }
    }

    private void BlockNetwork_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ProcessNetworkInfo processInfo)
        {
            if (NetworkBlocker.IsBlocked(processInfo.ProcessName))
            {
                // Unblock
                var result = CustomDialog.Show(
                    "Unblock Network",
                    $"Do you want to unblock network traffic for:\n\n{processInfo.ProcessName}?",
                    "✅");

                if (result)
                {
                    if (NetworkBlocker.UnblockProcess(processInfo.ProcessName))
                    {
                        CustomDialog.ShowMessage("Success", $"Network traffic unblocked for {processInfo.ProcessName}", "✅");
                    }
                    else
                    {
                        CustomDialog.ShowMessage("Error", "Failed to unblock network traffic.\n\nMake sure the app has admin privileges.", "⚠️");
                    }
                }
            }
            else
            {
                // Block
                var result = CustomDialog.Show(
                    "Block Network Traffic",
                    $"Do you want to block network traffic for:\n\n{processInfo.ProcessName} (PID: {processInfo.ProcessId})?\n\nThis requires administrator privileges.",
                    "🚫");

                if (result)
                {
                    if (NetworkBlocker.BlockProcess(processInfo.ProcessName, processInfo.ProcessId))
                    {
                        CustomDialog.ShowMessage("Success", $"Network traffic blocked for {processInfo.ProcessName}", "✅");
                    }
                    else
                    {
                        CustomDialog.ShowMessage("Error", "Failed to block network traffic.\n\nMake sure the app has admin privileges.", "⚠️");
                    }
                }
            }
        }
    }

    private void BlockNetworkGroup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ProcessGroupInfo groupInfo)
        {
            if (NetworkBlocker.IsBlocked(groupInfo.ProcessName))
            {
                // Unblock
                var result = CustomDialog.Show(
                    "Unblock Network",
                    $"Do you want to unblock network traffic for all instances of:\n\n{groupInfo.ProcessName}?",
                    "✅");

                if (result)
                {
                    if (NetworkBlocker.UnblockProcess(groupInfo.ProcessName))
                    {
                        CustomDialog.ShowMessage("Success", $"Network traffic unblocked for {groupInfo.ProcessName}", "✅");
                    }
                    else
                    {
                        CustomDialog.ShowMessage("Error", "Failed to unblock network traffic.", "⚠️");
                    }
                }
            }
            else
            {
                // Block
                var result = CustomDialog.Show(
                    "Block Network Traffic",
                    $"Do you want to block network traffic for all instances of:\n\n{groupInfo.ProcessName}?\n\nThis requires administrator privileges.",
                    "🚫");

                if (result)
                {
                    // Use first instance to get the executable path
                    var firstInstance = groupInfo.Instances.FirstOrDefault();
                    if (firstInstance != null)
                    {
                        if (NetworkBlocker.BlockProcess(groupInfo.ProcessName, firstInstance.ProcessId))
                        {
                            CustomDialog.ShowMessage("Success", $"Network traffic blocked for {groupInfo.ProcessName}", "✅");
                        }
                        else
                        {
                            CustomDialog.ShowMessage("Error", "Failed to block network traffic.\n\nMake sure the app has admin privileges.", "⚠️");
                        }
                    }
                }
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        updateTimer.Stop();
        processMonitor.Dispose();
        NetworkBlocker.BlockedProcessesChanged -= OnBlockedProcessesChanged;
    }
}
