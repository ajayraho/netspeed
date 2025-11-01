# 🚀 Network Speed Widget

A modern, real-time network monitoring widget for Windows 11 with process tracking, statistics visualization, and connection inspection.

![Network Speed Widget](https://img.shields.io/badge/Platform-Windows%2011-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features

### 📊 **Real-time Network Monitoring**

- Live upload/download speed display on floating widget
- Updates every second with accurate speed calculations
- System tray integration with speed tooltips
- Opacity animations when hovering
- Always-on-top positioning

### 🔍 **Process Network Tracking**

- Monitor network I/O per process in real-time
- Hierarchical grouping by process name
- View all instances of each process
- Sort by process name, download speed, or upload speed (ascending/descending)
- Process icons extracted from executables
- Shows PID for each process instance

### 🌐 **Connection Inspector**

- View all active TCP connections system-wide
- See which processes own which connections
- Remote IP addresses and port numbers displayed
- Automatic DNS hostname resolution (async)
- Connection state tracking (Established, TimeWait, etc.)
- Connection count per process
- Grouped by process with expand/collapse

### 📈 **Statistics & Graphs**

- Real-time line graphs for download and upload speeds
- Multiple time range options:
  - Last 60 seconds (default)
  - Last 5 minutes
  - Last 30 minutes
  - Last hour
- Session summary statistics:
  - Total data downloaded
  - Total data uploaded
  - Peak download speed
  - Peak upload speed
- Auto-scaling graphs with grid lines

### 🚫 **Network Traffic Control**

- Block network traffic for any process via Windows Firewall
- Unblock previously blocked processes
- Visual indicators (✅/🚫) showing block status
- Manage all blocked processes in dedicated window
- Real-time firewall rule scanning
- Persistent blocking rules (survives app restart)
- Works on both individual processes and all instances of a process

### 🎨 **User Interface**

- Modern dark theme throughout
- Tab-based organization (Processes, Connections, Statistics)
- Custom thin scrollbars (8px width)
- Draggable custom dialog boxes
- Centered dialogs on screen
- Smooth hover effects and transitions
- Message-only dialogs (OK button) vs confirmation dialogs (Yes/No)
- About dialog with app information

## 🎯 Requirements

- **OS**: Windows 10/11 (64-bit)
- **Runtime**: .NET 9.0 (included in self-contained build)
- **Permissions**: Administrator rights required for network blocking features

## 📥 Installation

### Option 1: Download Release

1. Download `NetworkSpeedWidget.exe` from [Releases](https://github.com/yourusername/NetworkSpeedWidget/releases/latest)
2. Run the executable (no installation required)
3. Right-click → Run as Administrator for full features (network blocking)

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

- **Double-click widget**: Opens detailed monitoring window
- **Hover over widget**: Widget becomes more visible
- **System tray icon**: Left-click to show/hide, right-click to exit

### Detailed Monitor Window

#### 📊 Processes Tab

- **Sort columns**: Click "Download" or "Upload" headers to sort (first click = descending)
- **Expand process**: Click ▶ arrow to see all instances
- **End Task**: Click ❌ button to kill process
- **Block Network**: Click 🚫 button to block/unblock (changes to ✅ when blocked)
- **Blocked Processes button**: Opens manager for all blocked processes
- **? button**: Opens About dialog

#### 🌐 Connections Tab

- **View connections**: See all active TCP connections per process
- **Expand process**: Click ▶ to see individual connections
- **Remote addresses**: Shows IP:Port and resolved hostnames
- **Connection state**: Displays TCP state (Established, etc.)

#### 📈 Statistics Tab

- **Live graphs**: Real-time download (green) and upload (red) speed charts
- **Time range dropdown**: Select 60s, 5m, 30m, or 1h history
- **Session stats**: View totals and peak speeds since window opened

### Blocked Processes Manager

- View all processes with active firewall blocks
- Click "Unblock" to remove individual blocks
- Click "Remove All Blocks" to clear all at once
- Click "Refresh" to sync with actual firewall rules

## 🔧 Building Release

To create a release build:

```powershell
# Clean previous builds
dotnet clean

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Output: bin\Release\net9.0-windows\win-x64\publish\NetworkSpeedWidget.exe
```

## 🛠️ Technical Implementation

### Core Technologies

- **.NET 9.0 WPF** - Modern Windows desktop framework
- **C# 12** - Latest C# language features
- **XAML** - Declarative UI markup

### Network Monitoring APIs

- **NetworkInterface.GetIPv4Statistics()** - Real-time byte counters for speed calculation
- **DateTime-based delta calculation** - Accurate speed measurement over time intervals

### Process Monitoring

- **Win32 GetProcessIoCounters** - Per-process I/O statistics via P/Invoke
- **Network factor heuristics** - Estimates network activity (30% for network apps, 10% for others)
- **Process.GetProcesses()** - System-wide process enumeration

### Connection Tracking

- **Win32 GetExtendedTcpTable** - Maps TCP connections to owning processes
- **IPGlobalProperties** - Retrieves active connection information
- **Dns.GetHostEntryAsync()** - Asynchronous hostname resolution

### Firewall Management

- **netsh advfirewall firewall** - Windows Firewall command-line interface
- **UAC elevation** - Prompts for admin rights when blocking
- **Rule naming convention**: `NetworkSpeedWidget_Block_{processName}_IN/OUT`

### UI Architecture

- **Observable Collections** - Auto-updating data bindings
- **INotifyPropertyChanged** - Real-time property change notifications
- **CollectionViewSource** - Persistent sorting with SortDescriptions
- **DispatcherTimer** - 1-second update intervals
- **Async/await** - Non-blocking background operations
- **Custom value converters** - Block state to icon/tooltip conversion

## 📋 What's Not Included

This is an accurate representation of what the app currently does. The following features are **NOT implemented**:

- Network interface selection (monitors all interfaces combined)
- Bandwidth limits or alerts
- Export to CSV/JSON
- Custom themes/colors
- UDP connection monitoring (only TCP)
- Historical data persistence (only session data)
- Auto-start with Windows
- Packet-level deep inspection

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📝 License

This project is licensed under the MIT License.

## ⚠️ Disclaimer

This software is provided as-is. Use at your own risk. Network blocking features require administrator privileges and modify Windows Firewall rules.

## 👨‍💻 Author

**Ajit Kumar**  
ajayraho productions

---

**Made with ❤️ for Windows power users**
