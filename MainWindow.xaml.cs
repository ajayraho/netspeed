using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace NetworkSpeedWidget;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private NetworkMonitor? networkMonitor;
    private DispatcherTimer? updateTimer;
    private System.Windows.Forms.NotifyIcon? notifyIcon;
    private DetailedMonitorWindow? detailedWindow;

    // Win32 API imports to hide from Alt+Tab
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    public MainWindow()
    {
        InitializeComponent();
        
        // Set window icon
        try
        {
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (System.IO.File.Exists(iconPath))
            {
                this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }
        }
        catch { }
        
        InitializeNetworkMonitor();
        InitializeSystemTray();
        InitializeTimer();
    }

    private void InitializeNetworkMonitor()
    {
        networkMonitor = new NetworkMonitor();
    }

    private void InitializeSystemTray()
    {
        notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Visible = true,
            Text = "Network Speed Widget"
        };

        // Try to load custom icon, fallback to system icon if not found
        try
        {
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (System.IO.File.Exists(iconPath))
            {
                notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            }
            else
            {
                notifyIcon.Icon = System.Drawing.SystemIcons.Information;
            }
        }
        catch
        {
            notifyIcon.Icon = System.Drawing.SystemIcons.Information;
        }

        notifyIcon.DoubleClick += (s, e) =>
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        };

        // Create context menu for system tray
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        var showHideItem = new System.Windows.Forms.ToolStripMenuItem("Show/Hide");
        showHideItem.Click += (s, e) =>
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }
        };

        var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) => ExitApplication();

        contextMenu.Items.Add(showHideItem);
        contextMenu.Items.Add(exitItem);
        notifyIcon.ContextMenuStrip = contextMenu;
    }

    private void InitializeTimer()
    {
        updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        updateTimer.Tick += UpdateTimer_Tick;
        updateTimer.Start();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            networkMonitor?.Update();

            if (networkMonitor != null)
            {
                DownloadSpeedText.Text = NetworkMonitor.FormatSpeed(networkMonitor.DownloadSpeed);
                UploadSpeedText.Text = NetworkMonitor.FormatSpeed(networkMonitor.UploadSpeed);

                // Update system tray tooltip
                if (notifyIcon != null)
                {
                    notifyIcon.Text = $"↓ {NetworkMonitor.FormatSpeed(networkMonitor.DownloadSpeed)}\n" +
                                     $"↑ {NetworkMonitor.FormatSpeed(networkMonitor.UploadSpeed)}";
                }
            }
            else
            {
                DownloadSpeedText.Text = "ERROR";
                UploadSpeedText.Text = "NULL";
            }
        }
        catch (Exception ex)
        {
            DownloadSpeedText.Text = "ERROR";
            UploadSpeedText.Text = ex.Message.Substring(0, Math.Min(10, ex.Message.Length));
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Hide from Alt+Tab
        var hwnd = new WindowInteropHelper(this).Handle;
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

        // Position window at bottom-right corner ON the taskbar
        var workingArea = SystemParameters.WorkArea;
        var fullScreen = new System.Drawing.Rectangle(
            (int)SystemParameters.VirtualScreenLeft,
            (int)SystemParameters.VirtualScreenTop,
            (int)SystemParameters.VirtualScreenWidth,
            (int)SystemParameters.VirtualScreenHeight
        );

        // Calculate taskbar position (difference between full screen and working area)
        var taskbarHeight = fullScreen.Height - workingArea.Height;
        var taskbarAtBottom = workingArea.Bottom < fullScreen.Bottom;

        if (taskbarAtBottom)
        {
            // Taskbar at bottom - position widget ON the taskbar
            this.Left = workingArea.Right - this.Width - 10;
            this.Top = workingArea.Bottom + 5; // Just inside the taskbar
        }
        else
        {
            // Taskbar at top or sides - fallback to working area
            this.Left = workingArea.Right - this.Width - 10;
            this.Top = workingArea.Bottom - this.Height - 10;
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Allow dragging the window
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Open detailed monitor window on double-click
        if (e.ChangedButton == MouseButton.Left)
        {
            if (detailedWindow == null || !detailedWindow.IsLoaded)
            {
                detailedWindow = new DetailedMonitorWindow();
                detailedWindow.Show();
            }
            else
            {
                detailedWindow.Activate();
                detailedWindow.WindowState = WindowState.Normal;
            }
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        ExitApplication();
    }

    private void ExitApplication()
    {
        updateTimer?.Stop();
        networkMonitor?.Dispose();
        notifyIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        updateTimer?.Stop();
        networkMonitor?.Dispose();
        notifyIcon?.Dispose();
    }
}
