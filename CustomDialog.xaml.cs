using System.Windows;
using System.Windows.Input;

namespace NetworkSpeedWidget;

public partial class CustomDialog : Window
{
    public bool Result { get; private set; }

    public CustomDialog(string title, string message, string icon = "⚠️", bool isMessageOnly = false)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
        IconText.Text = icon;

        // If message-only, hide No button and change Yes to OK
        if (isMessageOnly)
        {
            NoButton.Visibility = Visibility.Collapsed;
            YesButton.Content = "OK";
            YesButton.Width = 100;
        }
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void Yes_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void No_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    public static bool Show(string title, string message, string icon = "⚠️")
    {
        var dialog = new CustomDialog(title, message, icon);
        dialog.ShowDialog();
        return dialog.Result;
    }

    public static void ShowMessage(string title, string message, string icon = "ℹ️")
    {
        var dialog = new CustomDialog(title, message, icon, isMessageOnly: true);
        dialog.ShowDialog();
    }
}
