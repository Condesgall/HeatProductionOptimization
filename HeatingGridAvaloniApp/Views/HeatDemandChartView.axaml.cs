using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HeatingGridAvaloniApp.ViewModels;

namespace HeatingGridAvaloniApp.Views
{
    public partial class HeatDemandChartView : UserControl
    {
        public HeatDemandChartView()
        {
            InitializeComponent();
            DataContext = new HeatDemandChartViewModel();
        }
    }
}