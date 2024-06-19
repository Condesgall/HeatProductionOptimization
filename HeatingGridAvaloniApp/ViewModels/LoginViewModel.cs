using System;
using System.Reactive;
using ReactiveUI;
using Avalonia;
using Avalonia.Interactivity;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        // Reference to the MainWindowViewModel to allow navigation upon successful login
        private readonly MainWindowViewModel _mainWindowViewModel;

        // Fields to store user input for username and password, and an error message
        private string userName = string.Empty;
        private string password = string.Empty;
        private string errorMessage = string.Empty;
        private bool showPassword;

        // Constructor to initialize the ViewModel with a reference to MainWindowViewModel
        public LoginViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
            LoginCommand = ReactiveCommand.Create(Login);
        }

        // Property to bind the Username input from the UI
        public string UserName
        {
            get => userName;
            set => this.RaiseAndSetIfChanged(ref userName, value);
        }

        // Property to bind the Password input from the UI
        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        // Property to bind and display error messages in the UI
        public string ErrorMessage
        {
            get => errorMessage;
            set => this.RaiseAndSetIfChanged(ref errorMessage, value);
        }

        // Command to handle login logic
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        // Method to check the entered credentials and navigate if successful
        private void Login()
        {
            const string correctUsername = "Danfoss"; // Correct username
            const string correctPassword = "Danfoss"; // Correct password

            if (UserName == correctUsername && Password == correctPassword)
            {
                ErrorMessage = "Login successful!"; // Update the error message to indicate success
                _mainWindowViewModel.LoginButtonCommand(); // Call the command to navigate to the main application view
            }
            else
            {
                ErrorMessage = "Invalid username or password. Hint: Danfoss"; // Update the error message to indicate failure
            }
        }

        // Property to bind the checkbox state
        public bool ShowPassword
        {
            get => showPassword;
            set => this.RaiseAndSetIfChanged(ref showPassword, value);
        }
    }
}
