# Release Notes - Version 1.0.0

## 🎉 Initial Release

### Core Features

#### 📊 **Real-time Network Monitoring**

- Floating widget displays live upload/download speeds
- Updates every second with accurate byte delta calculations
- System tray integration with current speed tooltips
- Opacity animations (fades when not hovering)
- Always-on-top positioning above taskbar
- Auto-positions at bottom-right corner

#### 🔍 **Process Network Tracking**

- Monitors network I/O for all running processes
- Groups processes hierarchically by name
- Shows all instances of each process with individual PIDs
- Sortable columns (Name, Download, Upload)
- Default sort: Download speed descending
- Process icons extracted from executables
- Click to expand/collapse process groups
- Action buttons for each process:
  - **End Task**: Kill process with confirmation
  - **Block/Unblock Network**: Toggle firewall rules

#### 🌐 **Connection Inspector**

- Lists all active TCP connections system-wide
- Maps connections to owning processes using Win32 API
- Displays remote IP addresses and port numbers
- Asynchronous DNS hostname resolution
- Shows connection states (Established, TimeWait, etc.)
- Connection count per process
- Expandable tree view grouped by process

#### 📈 **Statistics & Graphs**

- Real-time line charts for download (green) and upload (red)
- Auto-scaling graphs with grid lines and value labels
- Configurable time ranges:
  - 60 seconds (default)
  - 5 minutes
  - 30 minutes
  - 1 hour
- Session statistics tracked since window opened:
  - Total downloaded (cumulative bytes)
  - Total uploaded (cumulative bytes)
  - Peak download speed
  - Peak upload speed
- Legend and professional formatting

#### 🚫 **Network Traffic Control**

- Block any process from accessing the network
- Creates Windows Firewall rules (inbound + outbound)
- Unblock previously blocked processes
- Visual indicators show block status (🚫 → ✅)
- Dedicated "Blocked Processes" manager window
- Features:
  - List all currently blocked processes
  - Unblock individual processes
  - Remove all blocks at once
  - Refresh button scans actual firewall rules
  - Empty state message when no blocks
- Real-time firewall rule scanning (no stale entries)
- Requires administrator privileges

#### 🎨 **User Interface**

- Modern dark theme (#1E1E1E background)
- Tab-based detailed monitor:
  - 📊 Processes
  - 🌐 Connections
  - 📈 Statistics
- Custom thin scrollbars (8px width)
- Hover effects (darker → lighter gray)
- Draggable custom dialogs
- Two dialog types:
  - Confirmation dialogs (Yes/No buttons)
  - Message dialogs (OK button only)
- Centered dialogs on screen
- Smooth animations throughout
- About dialog (? button) with:
  - Version information
  - Features list
  - Technical details
  - Author/company info
  - License info
  - GitHub link

### Technical Specifications

- **Platform**: Windows 10/11 (64-bit)
- **Framework**: .NET 9.0 WPF
- **Runtime**: Self-contained (includes .NET runtime)
- **Architecture**: x64
- **Single-file**: Yes
- **Compressed**: Yes
- **Size**: ~75 MB

### APIs & Technologies Used

- `NetworkInterface.GetIPv4Statistics()` - Network speed monitoring
- `Win32 GetProcessIoCounters` - Process I/O statistics via P/Invoke
- `Win32 GetExtendedTcpTable` - TCP connection to process mapping
- `netsh advfirewall firewall` - Windows Firewall management
- `Dns.GetHostEntryAsync()` - Asynchronous DNS resolution
- `Process.GetProcesses()` - System process enumeration
- Icon extraction from executables
- Observable collections with data binding
- CollectionViewSource for persistent sorting
- DispatcherTimer for 1-second updates

### Known Limitations

- **Process I/O monitoring**: Uses heuristic estimation (30% network factor for known network apps, 10% for others)
- **TCP only**: UDP connections not monitored
- **No historical data**: Statistics reset when window closes
- **Single interface**: Cannot select specific network adapter
- **Admin required**: Network blocking features need elevated privileges
- **Firewall-based blocking**: Only works via Windows Firewall

### System Requirements

- Windows 10 version 1809 or later (recommended: Windows 11)
- 4 GB RAM minimum
- Administrator rights for network blocking features
- No separate .NET installation needed (self-contained)

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
