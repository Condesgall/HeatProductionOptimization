using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HeatingGridAvaloniApp.ViewModels;
using HeatingGridAvaloniApp.Views;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;

namespace HeatingGridAvaloniApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // LiveCharts
        LiveCharts.Configure(config =>
                config
                    // Configuration for LiveCharts
                    .HasMap<City>((city, index) => new(index, city.Population))
            );
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    public record City(string Name, double Population);
}