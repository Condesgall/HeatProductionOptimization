using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using HeatingGridAvaloniApp.ViewModels;
using HeatingGridAvaloniApp.Views;
using LiveChartsCore.Kernel.Sketches;

namespace HeatingGridAvaloniApp.Views
{
    public partial class ChartView : UserControl
    {
        public ChartView()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}