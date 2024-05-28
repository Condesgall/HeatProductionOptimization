using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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

            // Run the delay asynchronously without marking the method as async
            Task.Run(async () =>
            {
                await Task.Delay(100); // Add a delay of 1 second (1000 milliseconds)

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    openedTab.Content = new VisualizerView();
                });
            });
        }

        public void SaveWeights(object sender, RoutedEventArgs e)
        {
            var saveButton = this.FindControl<Button>("SaveWeightsButton");
            saveButton.Content = "Saved";
        }
    }
}
