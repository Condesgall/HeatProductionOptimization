using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.Generic;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class VisualizerViewModel
    {
        public IEnumerable<ISeries> Series { get; set; }

        public VisualizerViewModel()
        {
            // Example data initialization
            Series = new List<ISeries>
            {
                new LineSeries<double> { Values = new double[] { 3, 5, 7, 9, 4, 6, 8 } }
            };
        }
    }
}