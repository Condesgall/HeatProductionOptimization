using System;
using ReactiveUI;
using AvaloniaSample;

namespace HeatingGridAvaloniApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Login = new LoginViewModel();
        MainApp = new MainAppViewModel();
        _contentViewModel = Login;

        // Initialize the chart ViewModel
        ChartData = new ViewModel();
    }

    public LoginViewModel Login { get; } 
    public MainAppViewModel MainApp { get; }
    private ViewModelBase _contentViewModel;
    public ViewModelBase ContentViewModel { 
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }

    //Property for the chart ViewModel
    public ViewModel ChartData { get; }
    public void LoginButtonCommand()
    {
        ContentViewModel = MainApp;
    }
}
