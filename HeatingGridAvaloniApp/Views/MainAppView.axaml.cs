using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HeatingGridAvaloniaApp.Views;

namespace HeatingGridAvaloniApp.Views;

public partial class MainAppView : UserControl
{
    public MainAppView()
    {
        InitializeComponent();
    }

    public void RedirectToAM(object sender, RoutedEventArgs e)
    {
        var contentArea = this.FindControl<ContentControl>("ContentArea");
        contentArea.Content = new AM_View();
    }

    public void RedirectToOptimizer(object sender, RoutedEventArgs e)
    {
        var contentArea = this.FindControl<ContentControl>("ContentArea");
        contentArea.Content = new AM_View();
    }
}