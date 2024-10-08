using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using HeatingGridAvaloniaApp.Models;
using HeatingGridAvaloniApp.Views;

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
        
        if (openedTab != null)
        {
            openedTab.Content = new AM_View();
        }
    }

    public void RedirectToOptimizer(object sender, RoutedEventArgs e)
    {
        var openedTab = this.FindControl<ContentControl>("OpenedTab");
        
        if (openedTab != null)
        {
            openedTab.Content = new OptimizerView();
        }
    }
}