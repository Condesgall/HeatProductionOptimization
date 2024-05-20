using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.IO;

namespace HeatingGridAvaloniApp.ViewModels
{
    public partial class HeatDemandChartViewModel : ObservableObject
    {
        static IEnumerable<int> ReadProducedHeat(string filename, string unitname)
        {
            List<int> producedHeat = new List<int>();
            using (var reader = new StreamReader(filename))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values[2] == unitname)
                    {
                        producedHeat.Add(int.Parse(values[3]));
                    }
                }
            }
            return producedHeat;
        }

        public ISeries[] Series { get; set; } =
        {
            new StackedColumnSeries<int>
            {
                Values = ReadProducedHeat("ResultData.csv", "GB"),
                Stroke = null,
                DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
                DataLabelsSize = 14,
                DataLabelsPosition = DataLabelsPosition.Middle
            },
            new StackedColumnSeries<int>
            {
                Values = ReadProducedHeat("ResultData.csv", "GM"),
                Stroke = null,
                DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
                DataLabelsSize = 14,
                DataLabelsPosition = DataLabelsPosition.Middle
            },
            new StackedColumnSeries<int>
            {
                Values = ReadProducedHeat("ResultData.csv", "OB"),
                Stroke = null,
                DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
                DataLabelsSize = 14,
                DataLabelsPosition = DataLabelsPosition.Middle
            },
            new StackedColumnSeries<int>
            {
                Values = ReadProducedHeat("ResultData.csv", "EK"),
                Stroke = null,
                DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
                DataLabelsSize = 14,
                DataLabelsPosition = DataLabelsPosition.Middle
            }
        };
    }
}
