using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HeatingGridAvaloniApp.ViewModels;

namespace HeatingGridAvaloniaApp.Views
{
    public partial class OptimizerView : UserControl
    {
        public OptimizerView()
        {
            InitializeComponent();
            DataContext = new OptimizerViewModel();
        }

        public void SaveClicked(object sender, RoutedEventArgs e)
        {
            var saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Content = "Saved";
        }
    }
}
