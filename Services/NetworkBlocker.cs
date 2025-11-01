using System.Diagnostics;
using System.IO;

namespace NetworkSpeedWidget.Services;

public class NetworkBlocker
{
    private static readonly string BlockedProcessesFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NetworkSpeedWidget",
        "blocked_processes.txt"
    );

    private static HashSet<string> blockedProcesses = new HashSet<string>();
    public static event Action? BlockedProcessesChanged;

    static NetworkBlocker()
    {
        LoadBlockedProcesses();
    }

    public static IReadOnlyCollection<string> GetBlockedProcesses()
    {
        // Refresh from actual firewall rules
        RefreshFromFirewall();
        return blockedProcesses.ToList().AsReadOnly();
    }

    private static void RefreshFromFirewall()
    {
        try
        {
            // Get all firewall rules created by our app
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "advfirewall firewall show rule name=all",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse output to find rules created by our app
            var foundProcesses = new HashSet<string>();
            var lines = output.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.StartsWith("Rule Name:", StringComparison.OrdinalIgnoreCase))
                {
                    var ruleName = line.Substring("Rule Name:".Length).Trim();
                    
                    // Check if it's our rule format: NetworkSpeedWidget_Block_PROCESSNAME_OUT or _IN
                    if (ruleName.StartsWith("NetworkSpeedWidget_Block_", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract process name from rule name
                        var parts = ruleName.Split('_');
                        if (parts.Length >= 4)
                        {
                            // Format: NetworkSpeedWidget_Block_PROCESSNAME_OUT/IN
                            var processName = parts[2];
                            foundProcesses.Add(processName.ToLower());
                        }
                    }
                }
            }

            // Update our cache with actual firewall state
            blockedProcesses = foundProcesses;
            SaveBlockedProcesses();
            
            Debug.WriteLine($"Refreshed from firewall: {blockedProcesses.Count} blocked processes");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing from firewall: {ex.Message}");
        }
    }

    public static bool IsBlocked(string processName)
    {
        return blockedProcesses.Contains(processName.ToLower());
    }

    public static bool BlockProcess(string processName, int processId)
    {
        try
        {
            // Get the executable path
            var process = Process.GetProcessById(processId);
            var exePath = process.MainModule?.FileName;

            if (string.IsNullOrEmpty(exePath))
            {
                Debug.WriteLine($"Could not get executable path for {processName}");
                return false;
            }

            // Create firewall rule using netsh
            var ruleName = $"NetworkSpeedWidget_Block_{processName}";
            
            // Block outbound traffic
            var outboundCommand = $"advfirewall firewall add rule name=\"{ruleName}_OUT\" dir=out program=\"{exePath}\" action=block";
            var outResult = RunNetsh(outboundCommand);

            // Block inbound traffic
            var inboundCommand = $"advfirewall firewall add rule name=\"{ruleName}_IN\" dir=in program=\"{exePath}\" action=block";
            var inResult = RunNetsh(inboundCommand);

            if (outResult && inResult)
            {
                blockedProcesses.Add(processName.ToLower());
                SaveBlockedProcesses();
                BlockedProcessesChanged?.Invoke();
                Debug.WriteLine($"Successfully blocked {processName}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error blocking process: {ex.Message}");
            return false;
        }
    }

    public static bool UnblockProcess(string processName)
    {
        try
        {
            var ruleName = $"NetworkSpeedWidget_Block_{processName}";
            
            // Remove outbound rule
            var outboundCommand = $"advfirewall firewall delete rule name=\"{ruleName}_OUT\"";
            var outResult = RunNetsh(outboundCommand);

            // Remove inbound rule
            var inboundCommand = $"advfirewall firewall delete rule name=\"{ruleName}_IN\"";
            var inResult = RunNetsh(inboundCommand);

            if (outResult || inResult) // Consider success if at least one rule was removed
            {
                blockedProcesses.Remove(processName.ToLower());
                SaveBlockedProcesses();
                BlockedProcessesChanged?.Invoke();
                Debug.WriteLine($"Successfully unblocked {processName}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error unblocking process: {ex.Message}");
            return false;
        }
    }

    private static bool RunNetsh(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas", // Request admin elevation
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error running netsh: {ex.Message}");
            return false;
        }
    }

    private static void LoadBlockedProcesses()
    {
        try
        {
            if (File.Exists(BlockedProcessesFile))
            {
                var lines = File.ReadAllLines(BlockedProcessesFile);
                blockedProcesses = new HashSet<string>(lines.Select(l => l.ToLower()));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading blocked processes: {ex.Message}");
        }
    }

    private static void SaveBlockedProcesses()
    {
        try
        {
            var directory = Path.GetDirectoryName(BlockedProcessesFile);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(BlockedProcessesFile, blockedProcesses);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving blocked processes: {ex.Message}");
        }
    }
}
