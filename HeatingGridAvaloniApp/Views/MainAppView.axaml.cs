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
        var openedTab = this.FindControl<ContentControl>("OpenedTab");
        
        openedTab.Content = new AM_View();
    }

    public void RedirectToOptimizer(object sender, RoutedEventArgs e)
    {
        var openedTab = this.FindControl<ContentControl>("OpenedTab");
        
        openedTab.Content = new OptimizerView();
    }
}