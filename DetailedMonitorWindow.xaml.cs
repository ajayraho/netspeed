using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Shapes;
using NetworkSpeedWidget.Services;
using NetworkSpeedWidget.Models;

namespace NetworkSpeedWidget;

public partial class DetailedMonitorWindow : Window
{
    private readonly ProcessMonitor processMonitor;
    private readonly NetworkMonitor networkMonitor;
    private readonly ConnectionMonitor connectionMonitor;
    private readonly DispatcherTimer updateTimer;
    private bool sortDescending = true;
    private string currentSortColumn = "";
    private ICollectionView processGroupsView;
    private bool isFirstUpdate = true;

    // Statistics tracking
    private readonly List<double> downloadHistory = new();
    private readonly List<double> uploadHistory = new();
    private double totalDownloaded = 0;
    private double totalUploaded = 0;
    private double peakDownload = 0;
    private double peakUpload = 0;
    private int maxHistoryPoints = 60; // Default to 60 seconds
    private DateTime sessionStart = DateTime.Now;

    public DetailedMonitorWindow()
    {
        InitializeComponent();
        
        processMonitor = new ProcessMonitor();
        networkMonitor = new NetworkMonitor();
        connectionMonitor = new ConnectionMonitor();
        
        // Use CollectionViewSource for sorting
        processGroupsView = CollectionViewSource.GetDefaultView(processMonitor.ProcessGroups);
        ProcessTreeView.ItemsSource = processGroupsView;

        // Set connections view
        ConnectionsTreeView.ItemsSource = connectionMonitor.ProcessConnections;

        // Set default sort to download speed descending
        // Don't set currentSortColumn so first click will sort descending
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
        
        // Update network statistics
        networkMonitor.Update();
        UpdateStatistics(networkMonitor.DownloadSpeed, networkMonitor.UploadSpeed);

        // Update connections (run in background)
        Task.Run(() => connectionMonitor.Update());
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
                    
                    // Update connection count
                    var totalConnections = connectionMonitor.ProcessConnections.Sum(p => p.ConnectionCount);
                    ConnectionCountText.Text = $"({connectionMonitor.ProcessConnections.Count} processes, {totalConnections} connections)";
                    
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
            // Toggle if clicking the same column
            sortDescending = !sortDescending;
        }
        else
        {
            // New column - always start with descending
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

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog
        {
            Owner = this
        };
        aboutDialog.ShowDialog();
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
        networkMonitor.Dispose();
        connectionMonitor.Dispose();
        NetworkBlocker.BlockedProcessesChanged -= OnBlockedProcessesChanged;
    }

    #region Connections Tab

    private void ConnectionExpander_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is ProcessConnectionInfo processConn)
        {
            processConn.ToggleExpanded();
            e.Handled = true;
        }
    }

    #endregion

    #region Statistics Tab

    private void UpdateStatistics(double downloadSpeed, double uploadSpeed)
    {
        // Add to history
        downloadHistory.Add(downloadSpeed);
        uploadHistory.Add(uploadSpeed);

        // Trim history to max points
        if (downloadHistory.Count > maxHistoryPoints)
        {
            downloadHistory.RemoveAt(0);
            uploadHistory.RemoveAt(0);
        }

        // Update totals (speed is bytes/sec, so multiply by 1 second)
        totalDownloaded += downloadSpeed;
        totalUploaded += uploadSpeed;

        // Update peaks
        if (downloadSpeed > peakDownload)
            peakDownload = downloadSpeed;
        if (uploadSpeed > peakUpload)
            peakUpload = uploadSpeed;

        // Update UI
        TotalDownloadedText.Text = FormatBytes(totalDownloaded);
        TotalUploadedText.Text = FormatBytes(totalUploaded);
        PeakDownloadText.Text = FormatSpeed(peakDownload);
        PeakUploadText.Text = FormatSpeed(peakUpload);

        // Redraw chart
        DrawSpeedChart();
    }

    private void DrawSpeedChart()
    {
        SpeedChart.Children.Clear();

        if (downloadHistory.Count == 0 || SpeedChart.ActualWidth == 0 || SpeedChart.ActualHeight == 0)
            return;

        var width = SpeedChart.ActualWidth;
        var height = SpeedChart.ActualHeight;
        var pointCount = downloadHistory.Count;

        // Find max value for scaling
        var maxValue = Math.Max(
            downloadHistory.Count > 0 ? downloadHistory.Max() : 0,
            uploadHistory.Count > 0 ? uploadHistory.Max() : 0
        );

        if (maxValue == 0)
            maxValue = 1; // Prevent division by zero

        // Add some padding (20% above max)
        maxValue *= 1.2;

        // Draw grid lines
        DrawGridLines(width, height, maxValue);

        // Draw download line (green)
        DrawLine(downloadHistory, width, height, maxValue, System.Windows.Media.Brushes.Green, 2);

        // Draw upload line (red)
        DrawLine(uploadHistory, width, height, maxValue, System.Windows.Media.Brushes.Red, 2);

        // Draw legend
        DrawLegend(width);
    }

    private void DrawGridLines(double width, double height, double maxValue)
    {
        var gridBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 255, 255, 255));
        var textBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(120, 255, 255, 255));

        // Horizontal grid lines (5 lines)
        for (int i = 0; i <= 4; i++)
        {
            var y = height - (height / 4 * i);
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = width,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            SpeedChart.Children.Add(line);

            // Add value label
            var value = maxValue / 4 * i;
            var label = new TextBlock
            {
                Text = FormatSpeed(value),
                Foreground = textBrush,
                FontSize = 10,
                FontFamily = new System.Windows.Media.FontFamily("Consolas")
            };
            Canvas.SetLeft(label, 5);
            Canvas.SetTop(label, y - 15);
            SpeedChart.Children.Add(label);
        }
    }

    private void DrawLine(List<double> data, double width, double height, double maxValue, System.Windows.Media.Brush brush, double thickness)
    {
        if (data.Count < 2)
            return;

        var pointCount = data.Count;
        var xStep = width / Math.Max(maxHistoryPoints - 1, 1);

        var polyline = new Polyline
        {
            Stroke = brush,
            StrokeThickness = thickness,
            StrokeLineJoin = PenLineJoin.Round
        };

        // Calculate starting x position (align to right)
        var startX = width - (pointCount - 1) * xStep;

        for (int i = 0; i < pointCount; i++)
        {
            var x = startX + (i * xStep);
            var y = height - (data[i] / maxValue * height);
            polyline.Points.Add(new System.Windows.Point(x, y));
        }

        SpeedChart.Children.Add(polyline);
    }

    private void DrawLegend(double width)
    {
        var legendX = width - 150;
        var legendY = 10;

        // Download legend
        var downloadRect = new System.Windows.Shapes.Rectangle
        {
            Width = 15,
            Height = 3,
            Fill = System.Windows.Media.Brushes.Green
        };
        Canvas.SetLeft(downloadRect, legendX);
        Canvas.SetTop(downloadRect, legendY + 5);
        SpeedChart.Children.Add(downloadRect);

        var downloadLabel = new TextBlock
        {
            Text = "Download",
            Foreground = System.Windows.Media.Brushes.White,
            FontSize = 11
        };
        Canvas.SetLeft(downloadLabel, legendX + 20);
        Canvas.SetTop(downloadLabel, legendY);
        SpeedChart.Children.Add(downloadLabel);

        // Upload legend
        var uploadRect = new System.Windows.Shapes.Rectangle
        {
            Width = 15,
            Height = 3,
            Fill = System.Windows.Media.Brushes.Red
        };
        Canvas.SetLeft(uploadRect, legendX);
        Canvas.SetTop(uploadRect, legendY + 25);
        SpeedChart.Children.Add(uploadRect);

        var uploadLabel = new TextBlock
        {
            Text = "Upload",
            Foreground = System.Windows.Media.Brushes.White,
            FontSize = 11
        };
        Canvas.SetLeft(uploadLabel, legendX + 20);
        Canvas.SetTop(uploadLabel, legendY + 20);
        SpeedChart.Children.Add(uploadLabel);
    }

    private void TimeRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TimeRangeComboBox == null)
            return;

        maxHistoryPoints = TimeRangeComboBox.SelectedIndex switch
        {
            0 => 60,        // 60 seconds
            1 => 300,       // 5 minutes
            2 => 1800,      // 30 minutes
            3 => 3600,      // 1 hour
            _ => 60
        };

        // Trim existing history if needed
        while (downloadHistory.Count > maxHistoryPoints)
        {
            downloadHistory.RemoveAt(0);
            uploadHistory.RemoveAt(0);
        }
    }

    private string FormatSpeed(double bytesPerSecond)
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

    private string FormatBytes(double bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    #endregion
}
