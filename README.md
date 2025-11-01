# 🚀 Network Speed Widget

A modern, real-time network monitoring widget for Windows 11 with process tracking, statistics visualization, and connection inspection.

![Network Speed Widget](https://img.shields.io/badge/Platform-Windows%2011-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features

### 📊 **Real-time Monitoring**
- Live network upload/download speeds
- Floating widget with opacity animations
- System tray integration
- Always-on-top display

### 🔍 **Process Monitoring**
- Track network usage per process
- Hierarchical grouping by process name
- Sort by download/upload speed
- Process icons and instance counts
- End task and block network traffic

### 🌐 **Connection Inspector**
- View all active TCP connections
- See which IPs/domains processes connect to
- Connection count per process
- DNS hostname resolution
- Real-time connection state monitoring

### � **Statistics & Analytics**
- Real-time speed graphs (download/upload)
- Configurable time ranges (60s, 5m, 30m, 1h)
- Session statistics (total downloaded/uploaded)
- Peak speed tracking
- Beautiful chart visualizations

### 🚫 **Network Control**
- Block/unblock network traffic per process
- Windows Firewall integration
- Manage all blocked processes
- Visual indicators for blocked state

### 🎨 **Modern UI**
- Dark theme interface
- Custom thin scrollbars
- Smooth animations
- Draggable dialogs
- Responsive design

## 🎯 Requirements

- **OS**: Windows 10/11 (64-bit)
- **Runtime**: .NET 9.0 (included in single-file build)
- **Permissions**: Administrator rights (for blocking network traffic)

## 📥 Installation

### Option 1: Download Release (Recommended)
1. Download `NetworkSpeedWidget.exe` from [Releases](https://github.com/yourusername/NetworkSpeedWidget/releases)
2. Run the executable
3. (Optional) Right-click → Run as Administrator for network blocking features

### Option 2: Build from Source
```powershell
# Clone the repository
git clone https://github.com/yourusername/NetworkSpeedWidget.git
cd NetworkSpeedWidget

# Build
dotnet build

# Run
dotnet run
```

## 🚀 Usage

### Main Widget
- **Double-click**: Open detailed monitoring window
- **Hover**: Shows current speeds
- **Opacity**: Automatically adjusts based on mouse position

### Detailed Monitor Window

#### Processes Tab
- Click column headers to sort
- Click ▶ to expand process instances
- **🚫 Button**: Block/unblock network traffic
- **❌ Button**: End process task

#### Connections Tab
- View all active connections per process
- Expand to see remote IPs and hostnames
- Monitor connection states

#### Statistics Tab
- Live speed graphs
- Change time range from dropdown
- View session totals and peak speeds

### System Tray
- **Left-click**: Show/hide widget
- **Right-click**: Exit application

## 🔧 Building Release

To create a release build:

```powershell
# Clean previous builds
dotnet clean

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Output: bin\Release\net9.0-windows\win-x64\publish\NetworkSpeedWidget.exe
```

## 🛠️ Technical Details

### Technologies
- **Framework**: .NET 9.0 WPF
- **Language**: C# 12
- **APIs Used**:
  - `NetworkInterface` - Network speed monitoring
  - `GetProcessIoCounters` (Win32) - Process I/O statistics
  - `GetExtendedTcpTable` (Win32) - Connection tracking
  - `netsh advfirewall` - Firewall management

### Architecture
- **MVVM Pattern**: Clean separation of concerns
- **Observable Collections**: Real-time UI updates
- **Async/Await**: Non-blocking operations
- **P/Invoke**: Native Windows API integration

## 📋 Features Roadmap

- [ ] Network interface selection
- [ ] Bandwidth limits and alerts
- [ ] Export statistics to CSV
- [ ] Custom themes
- [ ] UDP connection monitoring
- [ ] Historical data tracking

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📝 License

This project is licensed under the MIT License.

## ⚠️ Disclaimer

This software is provided as-is. Use at your own risk. Network blocking features require administrator privileges and modify Windows Firewall rules.

---

**Made with ❤️ for Windows power users**
