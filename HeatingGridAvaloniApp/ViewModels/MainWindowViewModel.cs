﻿using System;
using ReactiveUI;
namespace HeatingGridAvaloniApp.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        Login = new LoginViewModel(this);
        MainApp = new MainAppViewModel();
        _contentViewModel = Login;
    }
    public LoginViewModel Login { get; } 
    public MainAppViewModel MainApp { get; }
    private ViewModelBase _contentViewModel;
    public ViewModelBase ContentViewModel { 
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
    public void LoginButtonCommand()
    {
        ContentViewModel = MainApp;
    }

    public string ImagePath
    {
        get => "/Assets/danfossLogo.png";
    }
}
