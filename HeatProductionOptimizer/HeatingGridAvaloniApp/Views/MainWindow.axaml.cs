using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace HeatingGridAvaloniApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void ButtonClicked(object source, RoutedEventArgs args)
    {
        Debug.WriteLine($"Click! UserName={userName.Text}");

        if (double.TryParse(userName.Text, out double C))
        {
            var F = C * (9d / 5d) + 32;
            password.Text = F.ToString("0.0");
        }
        else
        {
            userName.Text = "0";
            password.Text = "0";
        }
    }
}