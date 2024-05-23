using System;
using HeatingGridAvaloniaApp.Models;
using ReactiveUI;

namespace HeatingGridAvaloniApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Login = new LoginViewModel();
        MainApp = new MainAppViewModel();
        ChartData = new ViewModel();
        Optimizer = new OptimizerViewModel(this);

        _contentViewModel = Login;
    }

    public LoginViewModel Login { get; } 
    public MainAppViewModel MainApp { get; }
    public ViewModel ChartData { get; }
    public OptimizerViewModel Optimizer { get; }
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
        ContentViewModel = new ViewModel();
    }

    public void ShowOptimizerView()
    {
        ContentViewModel = Optimizer as ViewModelBase;
    }
}
