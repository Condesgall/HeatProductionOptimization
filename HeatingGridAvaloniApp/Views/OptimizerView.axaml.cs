using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using HeatingGridAvaloniApp.ViewModels;
using HeatingGridAvaloniApp.Views;

namespace HeatingGridAvaloniApp.Views
{
    public partial class OptimizerView : UserControl
    {
        OptimizerViewModel optimizerViewModel = new OptimizerViewModel();
        public OptimizerView()
        {
            InitializeComponent();
            DataContext = new OptimizerViewModel();
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            var saveButton = this.FindControl<Button>("SaveButton");
            saveButton.Content = "Saved";
        }

        public void Visualize(object sender, RoutedEventArgs e)
        {
            var openedTab = this.FindControl<ContentControl>("OpenedTab");
            openedTab.Content = new VisualizerView();
        }

        public void SaveWeights(object sender, RoutedEventArgs e)
        {
            var saveButton = this.FindControl<Button>("SaveWeightsButton");
            saveButton.Content = "Saved";
        }
    }
}
