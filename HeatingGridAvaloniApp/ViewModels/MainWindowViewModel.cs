using System;
using ReactiveUI;

namespace HeatingGridAvaloniApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {

    }

    public LoginViewModel Login { get; } 
    private ViewModelBase _contentViewModel;
    public ViewModelBase ContentViewModel { 
        get => _contentViewModel;
        set => this.RaiseAndSetIfChanged(ref _contentViewModel, value);
    }
}
