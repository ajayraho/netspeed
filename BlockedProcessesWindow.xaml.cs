using System.Windows;
using NetworkSpeedWidget.Services;

namespace NetworkSpeedWidget;

public partial class BlockedProcessesWindow : Window
{
    public BlockedProcessesWindow()
    {
        InitializeComponent();
        // Force refresh from firewall on load
        LoadBlockedProcesses();
    }

    private void LoadBlockedProcesses()
    {
        // This now automatically scans firewall rules
        var blockedProcesses = NetworkBlocker.GetBlockedProcesses();
        
        BlockedProcessesList.ItemsSource = blockedProcesses;
        CountText.Text = $"({blockedProcesses.Count} process{(blockedProcesses.Count != 1 ? "es" : "")} blocked)";
        
        // Show empty state if no processes blocked
        if (blockedProcesses.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            BlockedProcessesList.Visibility = Visibility.Collapsed;
        }
        else
        {
            EmptyState.Visibility = Visibility.Collapsed;
            BlockedProcessesList.Visibility = Visibility.Visible;
        }
    }

    private void UnblockProcess_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string processName)
        {
            var result = CustomDialog.Show(
                "Unblock Process",
                $"Do you want to unblock network traffic for:\n\n{processName}?",
                "✅");

            if (result)
            {
                if (NetworkBlocker.UnblockProcess(processName))
                {
                    LoadBlockedProcesses(); // Refresh the list
                    CustomDialog.Show("Success", $"Network traffic unblocked for {processName}", "✅");
                }
                else
                {
                    CustomDialog.Show("Error", "Failed to unblock network traffic.\n\nMake sure the app has admin privileges.", "⚠️");
                }
            }
        }
    }

    private void RemoveAllBlocks_Click(object sender, RoutedEventArgs e)
    {
        var blockedProcesses = NetworkBlocker.GetBlockedProcesses();
        
        if (blockedProcesses.Count == 0)
        {
            CustomDialog.Show("No Blocks", "There are no blocked processes.", "ℹ️");
            return;
        }

        var result = CustomDialog.Show(
            "Remove All Blocks",
            $"Are you sure you want to unblock all {blockedProcesses.Count} process{(blockedProcesses.Count != 1 ? "es" : "")}?\n\nAll network restrictions will be removed.",
            "🗑️");

        if (result)
        {
            var successful = 0;
            var failed = 0;

            foreach (var processName in blockedProcesses.ToList())
            {
                if (NetworkBlocker.UnblockProcess(processName))
                    successful++;
                else
                    failed++;
            }

            LoadBlockedProcesses(); // Refresh the list

            if (failed == 0)
            {
                CustomDialog.Show("Success", $"All {successful} process{(successful != 1 ? "es" : "")} unblocked successfully!", "✅");
            }
            else
            {
                CustomDialog.Show("Partial Success", $"Unblocked: {successful}\nFailed: {failed}", "⚠️");
            }
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadBlockedProcesses();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
