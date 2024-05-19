using Avalonia;
using Avalonia.Controls;
using HeatingGridAvaloniApp.ViewModels;

namespace HeatingGridAvaloniaApp.Views
{
    public partial class AM_View : UserControl
    {
        public AM_View()
        {
            InitializeComponent();
            DataContext = new AM_ViewModel();
        }
    }
}
