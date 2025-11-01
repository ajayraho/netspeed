using System;
using System.Globalization;
using System.Windows.Data;
using NetworkSpeedWidget.Services;

namespace NetworkSpeedWidget.Converters;

public class BlockedStateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string processName)
        {
            var isBlocked = NetworkBlocker.IsBlocked(processName);
            
            if (parameter?.ToString() == "Icon")
            {
                return isBlocked ? "✅" : "🚫";
            }
            else if (parameter?.ToString() == "Tooltip")
            {
                return isBlocked ? "Unblock Network Traffic" : "Block Network Traffic";
            }
            else if (parameter?.ToString() == "Color")
            {
                return isBlocked ? "#FF4CAF50" : "#FFFF9800";
            }
        }
        
        return "🚫";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
