using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia;
using System.Dynamic;


namespace HeatingGridAvaloniApp.ViewModels
{
    public class OptimalizationChartViewModel : ViewModelBase
    {
        public ISeries[] Series { get; set; }
            = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
                    Fill = null
                }
            };
    }
}
