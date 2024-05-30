using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using HeatingGridAvaloniaApp.Models;

namespace HeatingGridAvaloniApp.ViewModels
{
    public class VisualizerViewModel
    {
        ResultDataManager resultDataManager = new ResultDataManager();

        static List<double> gbValues = new List<double>();
        static List<double> obValues = new List<double>();
        static List<double> gmValues = new List<double>();
        static List<double> ekValues = new List<double>();

        // Returns the maximum heat data for a given unit
        public static double GetValues(string unitName)
        {
            string filePath = "Assets/heatingGrids.csv";
            string[] values = File.ReadAllText(filePath).Split(',');

            switch (unitName)
            {
                case "GB":
                    return double.Parse(values[4]);
                case "OB":
                    return double.Parse(values[10]);
                case "GM":
                    return double.Parse(values[16]);
                case "EK":
                    return double.Parse(values[22]);
                default:
                    throw new ArgumentException($"Unit name {unitName} not found in the file.");
            }
        }

        // Reads data for the graph
        static void ReadProducedHeat(string filename)
        {
            // Clear the lists
            gbValues.Clear();
            obValues.Clear();
            gmValues.Clear();
            ekValues.Clear();

            using (var reader = new StreamReader(filename))
            {
                reader.ReadLine(); // Skip header line

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        var values = line.Split(',');

                        // Get the index where the value was added
                        int index = gbValues.Count; // Using gbValues.Count, assuming all lists are synchronized

                        // Check for combined unit names
                        bool isGb = values[2].Contains("GB");
                        bool isOb = values[2].Contains("OB");
                        bool isGm = values[2].Contains("GM");
                        bool isEk = values[2].Contains("EK");

                        // Insert NaN or actual values into each list
                        InsertValue(gbValues, index, isGb ? double.Parse(values[3]) :0);
                        InsertValue(obValues, index, isOb ? double.Parse(values[3]) : 0);
                        InsertValue(gmValues, index, isGm ? double.Parse(values[3]) :0);
                        InsertValue(ekValues, index, isEk ? double.Parse(values[3]) : 0);
                    }
                }
            }
        }

        // Helper function to insert a value into a list
        static void InsertValue(List<double> list, int index, double value)
        {
            if (index >= list.Count)
            {
                list.Add(value);
            }
            else
            {
                list.Insert(index, value);
            }
        }

        // Constructor
        public VisualizerViewModel()
        {
            ReadProducedHeat("Assets/ResultData.csv");

            // Graph logic
            Series = new ISeries[]
            {
               new StackedStepAreaSeries<double>
                {
                    Values = gbValues,
                    Fill = new SolidColorPaint(SKColors.Blue)
                },
                new StackedStepAreaSeries<double>
                {
                    Values = obValues,
                    Fill = new SolidColorPaint(SKColors.Orange)
                },
                new StackedStepAreaSeries<double>
                {
                    Values = gmValues,
                    Fill = new SolidColorPaint(SKColors.Red)
                },
                new StackedStepAreaSeries<double>
                {
                    Values = ekValues,
                    Fill = new SolidColorPaint(SKColors.Green)
                }
            };

            XAxes = new Axis[]
            {
                new Axis
                {
                    CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
                    CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
                    CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
                    Labeler = value => value.ToString("N2")
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
                    CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
                    CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
                    CrosshairSnapEnabled = true
                }
            };
        }

        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        public decimal NetCostAverage
        {
            get => resultDataManager.AverageNetCost;
            set => resultDataManager.AverageNetCost = Math.Round(value, 2);
        }

        public decimal Co2Average
        {
            get => resultDataManager.AverageCo2;
            set => resultDataManager.AverageCo2 = Math.Round(value, 2);
        }
    }
}
