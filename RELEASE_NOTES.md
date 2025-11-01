# Release Notes - Version 1.0.0

## 🎉 Initial Release

### Features

#### 📊 **Network Monitoring**
- Real-time upload/download speed display
- Floating widget with opacity animations
- System tray integration with speed tooltips
- Automatic positioning above taskbar

#### 🔍 **Process Monitoring**
- Track network usage per process
- Hierarchical grouping by process name
- Sort by name, download speed, or upload speed
- Process icons and instance counts
- Action buttons:
  - End Task (kill process)
  - Block/Unblock Network Traffic

#### 🌐 **Connection Inspector**
- View all active TCP connections
- See which IPs and domains each process connects to
- Connection count per process
- Automatic DNS hostname resolution
- Real-time connection state (Established, TimeWait, etc.)

#### 📈 **Statistics & Analytics**
- Real-time speed graphs (download and upload)
- Multiple time range options:
  - Last 60 seconds
  - Last 5 minutes
  - Last 30 minutes
  - Last hour
- Session statistics:
  - Total downloaded
  - Total uploaded
  - Peak download speed
  - Peak upload speed

#### 🚫 **Network Control**
- Block network traffic for specific processes
- Unblock previously blocked processes
- Manage all blocked processes in one place
- Windows Firewall integration
- Visual indicators for blocked state
- Persistent blocking rules

#### 🎨 **User Interface**
- Modern dark theme
- Custom thin scrollbars (8px)
- Smooth animations and transitions
- Draggable custom dialogs
- Responsive layout
- Tab-based organization:
  - 📊 Processes
  - 🌐 Connections
  - 📈 Statistics

### Technical Specifications

- **Platform**: Windows 10/11 (64-bit)
- **Runtime**: .NET 9.0 (self-contained)
- **Architecture**: x64
- **Single-file**: Yes (includes all dependencies)
- **Size**: ~45-50 MB (compressed)

### System Requirements

- Windows 10 version 1809 or later
- Windows 11 (recommended)
- 4 GB RAM minimum
- Administrator rights (for network blocking features)

### Known Limitations

- Network blocking requires administrator privileges
- Only TCP connections are monitored (UDP not yet supported)
- DNS resolution may be slow for some addresses
- Process I/O statistics use heuristic network factor estimation

### Installation

1. Download `NetworkSpeedWidget.exe`
2. Run the executable (no installation required)
3. (Optional) Right-click → Run as Administrator for full features

### First-time Setup

- Widget appears at bottom-right corner above taskbar
- Double-click widget to open detailed monitoring
- System tray icon provides quick access
- No configuration needed - works out of the box!

### Reporting Issues

If you encounter any issues, please report them on GitHub Issues with:
- Windows version
- Steps to reproduce
- Screenshots if applicable

---

**Thank you for using Network Speed Widget! 🚀**
