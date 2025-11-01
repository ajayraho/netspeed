using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetworkSpeedWidget.Utils;

public static class IconExtractor
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static ImageSource? GetProcessIcon(string processName, int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            var filePath = process.MainModule?.FileName;
            
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return GetDefaultIcon();
            }

            return ExtractIconFromFile(filePath);
        }
        catch
        {
            return GetDefaultIcon();
        }
    }

    public static ImageSource? ExtractIconFromFile(string filePath)
    {
        try
        {
            // Extract icon using Shell32
            IntPtr hIcon = ExtractIcon(IntPtr.Zero, filePath, 0);
            
            if (hIcon == IntPtr.Zero || hIcon == new IntPtr(1))
            {
                // Try using Icon.ExtractAssociatedIcon
                using var icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    return ConvertIconToImageSource(icon);
                }
                return GetDefaultIcon();
            }

            using var ico = Icon.FromHandle(hIcon);
            var imageSource = ConvertIconToImageSource(ico);
            DestroyIcon(hIcon);
            
            return imageSource;
        }
        catch
        {
            return GetDefaultIcon();
        }
    }

    private static ImageSource ConvertIconToImageSource(Icon icon)
    {
        using var bitmap = icon.ToBitmap();
        var hBitmap = bitmap.GetHbitmap();

        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private static ImageSource? GetDefaultIcon()
    {
        // Return a simple default icon (gear/cog)
        return null; // Will show empty space or default image
    }
}
