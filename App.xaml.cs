using System.Configuration;
using System.Data;
using System.Windows;

namespace NetworkSpeedWidget;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up unhandled exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            System.Windows.MessageBox.Show($"Unhandled Exception: {ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            System.Windows.MessageBox.Show($"UI Exception: {args.Exception.Message}\n\nStack Trace:\n{args.Exception.StackTrace}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}

