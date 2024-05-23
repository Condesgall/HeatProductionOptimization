using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using HeatingGridAvaloniApp.ViewModels;
using HeatingGridAvaloniApp.Views;
using LiveChartsCore.Kernel.Sketches;

namespace HeatingGridAvaloniaApp.Views
{
    public partial class OptimizerView : UserControl
    {
        public OptimizerView()
        {
            InitializeComponent();
            var mainWindow = Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow as MainWindow : null;
            DataContext = new OptimizerViewModel(mainWindow?.DataContext as MainWindowViewModel ?? new MainWindowViewModel());
        }

        public void SaveClicked(object sender, RoutedEventArgs e)
        {
            var saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Content = "Saved";
        }
    }
}
