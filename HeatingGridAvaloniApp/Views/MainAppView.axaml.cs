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
        var buttonPanel = this.FindControl<StackPanel>("ButtonPanel");
        var openedTab = this.FindControl<ContentControl>("OpenedTab");
        
        buttonPanel.IsVisible = false;
        openedTab.Content = new AM_View();
    }

    public void RedirectToOptimizer(object sender, RoutedEventArgs e)
    {
        var buttonPanel = this.FindControl<StackPanel>("ButtonPanel");
        var openedTab = this.FindControl<ContentControl>("OpenedTab");
        
        buttonPanel.IsVisible = false;
        openedTab.Content = new OptimizerView();
    }
}