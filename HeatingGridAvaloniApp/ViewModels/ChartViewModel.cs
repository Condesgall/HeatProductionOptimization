using System.Collections.Generic;
using System.Linq;
using HeatingGridAvaloniaApp.Models;
using HeatingGridAvaloniApp.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using ReactiveUI;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class ChartViewModel : ViewModelBase
    {
        private ISeries[] _series;
        public ISeries[] Series
        {
           get => _series;
           set => this.RaiseAndSetIfChanged(ref _series, value);
        }

        public void UpdateChartData(List<ResultData> filteredData)
        {
            Series = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Values = filteredData.Select(r => r.OptimizationResults.ProducedHeat).ToArray(),
                    Fill = null
                }
            };
        }
    }
}