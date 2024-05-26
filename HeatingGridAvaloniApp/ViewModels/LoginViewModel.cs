using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia;
using System.Dynamic;
using Avalonia.Interactivity;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        /*public MainWindowViewModel DataContext { get; private set; }
        private string userName;
        private string password;
        private string errorMessage;*/
        public LoginViewModel() 
        {

        }
        public string ImagePath
        {
            get => "/Assets/heater.jpg";
        }
        public string ImagePath2
        {
            get => "/Assets/background.pmh";
        }

        /*public string UserName
        {
            get => userName;
            set => this.RaiseAndSetIfChanged(ref userName, value);
        }

        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set => this.RaiseAndSetIfChanged(ref errorMessage, value);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainWindowViewModel viewModel)
            {
                viewModel.LoginButtonCommand(UserName, Password);
            }
        }*/
    }
}