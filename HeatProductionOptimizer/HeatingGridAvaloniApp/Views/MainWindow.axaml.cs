using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace HeatingGridAvaloniApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void ButtonClicked(object source, RoutedEventArgs args)
    {
        Debug.WriteLine($"Click! UserName={userName.Text}");

        string userNameInput = userName.Text;

        if (!string.IsNullOrEmpty(userNameInput))
        {
            if (userNameInput == "Danfoss")
            {
                password.Text = userNameInput + "EmployeeAccess";
            }
            else
            {
                password.Text = "UserAccess";
            }
        }
    }
}