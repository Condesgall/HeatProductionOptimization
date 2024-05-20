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
    }
}
