using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LiveChartsCore; 
using LiveChartsCore.Kernel; 
using LiveChartsCore.SkiaSharpView; 
using SkiaSharp;
using HeatingGridAvaloniApp.ViewModels;
using HeatingGridAvaloniaApp.Models;

namespace HeatingGridAvaloniApp.Views
{
    public partial class VisualizerView : UserControl
    {
        public VisualizerView()
        {
            InitializeComponent();
            DataContext = new VisualizerViewModel();
        }
    }
}
