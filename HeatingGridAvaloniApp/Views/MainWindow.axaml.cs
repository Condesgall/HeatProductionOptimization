using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HeatingGridAvaloniApp.ViewModels;
using System.Diagnostics;
using ReactiveUI;
using Avalonia.Markup.Xaml;

namespace HeatingGridAvaloniApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

}

