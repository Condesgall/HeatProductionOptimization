using System;
using HeatingGridAvaloniaApp.Models;
using HeatingGridAvaloniApp.Views;
using ReactiveUI;

namespace HeatingGridAvaloniApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Login = new LoginViewModel();
        MainApp = new MainAppViewModel();
        ChartData = new ChartViewModel();
        _contentViewModel = Login;
    }

    public LoginViewModel Login { get; } 
    public MainAppViewModel MainApp { get; }
    public ChartViewModel ChartData { get; }
    private ViewModelBase _contentViewModel;
    public ViewModelBase ContentViewModel 
    { 
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    public void LoginButtonCommand()
    {
        ContentViewModel = MainApp;
    }

    public void ShowChartView()
    {
        ContentViewModel = new ChartViewModel();
    }
}
