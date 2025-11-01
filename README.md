# Network Speed Widget for Windows 11

A lightweight, always-on-top network speed monitor for Windows 11 that displays real-time upload and download speeds near your taskbar.

## Features

✨ **Real-time Monitoring** - Updates network speeds every second
📊 **Dual Display** - Shows both download (↓) and upload (↑) speeds
🎯 **Always on Top** - Stays visible above all other windows
🖱️ **Draggable** - Click and drag to reposition anywhere on screen
💾 **System Tray Integration** - Minimizes to system tray with speed tooltip
🎨 **Sleek Design** - Semi-transparent dark theme with rounded corners
🚫 **No Taskbar Icon** - Keeps your taskbar clean

## Requirements

- Windows 11 (or Windows 10)
- .NET 9.0 Runtime
- Administrator privileges (for PerformanceCounter access)

## How to Run

### From Visual Studio Code:

```bash
dotnet run
```

### Build and Run Executable:

```bash
# Build the application
dotnet build -c Release

# Run the executable
.\bin\Release\net9.0-windows\NetworkSpeedWidget.exe
```

## Usage

### Window Controls:

- **Left Click + Drag** - Move the widget anywhere on screen
- **Right Click** - Open context menu with Exit option
- **System Tray Icon** - Double-click to show/hide widget

### System Tray:

- The widget creates a system tray icon
- Hover over the icon to see current speeds
- Right-click for Show/Hide and Exit options

## Default Position

The widget automatically positions itself at the bottom-right corner of your screen, just above the taskbar (with 10px padding).

## Speed Display Format

Speeds are automatically formatted:

- **B/s** - Bytes per second
- **KB/s** - Kilobytes per second
- **MB/s** - Megabytes per second
- **GB/s** - Gigabytes per second

## Project Structure

```
NetworkSpeedWidget/
├── App.xaml              # WPF Application definition
├── App.xaml.cs           # Application code-behind
├── MainWindow.xaml       # Main widget UI
├── MainWindow.xaml.cs    # Main window logic
├── NetworkMonitor.cs     # Network speed monitoring service
└── NetworkSpeedWidget.csproj  # Project configuration
```

## Technical Details

### Network Monitoring

- Uses `System.Diagnostics.PerformanceCounter` to monitor network interface statistics
- Automatically detects the active network interface
- Filters out loopback and virtual adapters
- Updates every 1 second via `DispatcherTimer`

### UI Design

- **Window Size**: 180×80 pixels
- **Background**: Semi-transparent black (#E0000000)
- **Border**: Subtle white border with 8px rounded corners
- **Fonts**: Consolas for monospaced speed display
- **Colors**:
  - Download: Green (#4CAF50)
  - Upload: Orange/Red (#FF5722)

## Troubleshooting

### No speeds showing (0 B/s):

1. Run the application as Administrator
2. Check your network connection
3. The first reading may take 1-2 seconds to appear

### Wrong network interface:

The app auto-detects the active interface. If it picks the wrong one, you may need to modify `NetworkMonitor.cs` to specify your interface name.

### System tray icon not appearing:

This is normal on some systems. The widget window will still function normally.

## Future Enhancements

- [ ] Custom theme colors
- [ ] Configurable update interval
- [ ] Network interface selection
- [ ] Data usage statistics
- [ ] Graph visualization
- [ ] Settings panel
- [ ] Auto-start with Windows

## License

Free to use and modify for personal or commercial projects.

## Author

Created for Windows 11 taskbar integration using C# and WPF.
