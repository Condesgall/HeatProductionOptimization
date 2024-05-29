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

        // Returns the maximum heat data for a given unit
        public static double GetValues(string unitName)
        {
            // File path containing the data
            string filePath = "Assets/heatingGrids.csv";
            // Read the CSV file and split the values
            string[] values = File.ReadAllText(filePath).Split(',');

            // Switch statement to extract the maximum heat data based on the unit name
            switch (unitName)
            {
                case "GB":
                    return double.Parse(values[4]); // Extract maximum heat for GB
                case "OB":
                    return double.Parse(values[10]); // Extract maximum heat for OB
                case "GM":
                    return double.Parse(values[16]); // Extract maximum heat for GM
                case "EK":
                    return double.Parse(values[22]); // Extract maximum heat for EK
                default:
                    throw new ArgumentException($"Unit name {unitName} not found in the file.");
            }
        }

        // Reads data for the graph
        static List<double> ReadProducedHeat(string filename, string unitname)
        {
            List<double> producedHeat = new List<double>();

            // Read the CSV file containing the produced heat data
            using (var reader = new StreamReader(filename))
            {
                reader.ReadLine(); // Skip header line

                // Read each line of the CSV file
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        var values = line.Split(',');

                        // Check if the unit name matches
                        if (values[2] == unitname)
                        {
                            producedHeat.Add(double.Parse(values[3])); // Add the produced heat value
                        }
                        // Check if the unit name is combined with another unit (e.g., "GB+OB")
                        else if (values[2] == $"{unitname}+GB" || values[2] == $"{unitname}+OB" || values[2] == $"{unitname}+EK" || values[2] == $"{unitname}+GM")
                        {
                            if (!(double.Parse(values[3]) <= GetValues(unitname)))
                            {
                                producedHeat.Add(GetValues(unitname)); // Add the maximum heat value for the unit
                            }else 
                            {   
                                producedHeat.Add(double.Parse(values[3]));
                            }
                        }
                        // Check if the unit name is combined with another unit (e.g., "GB+EK")
                        else if (values[2] == $"GB+{unitname}")
                        {
                            double value = double.Parse(values[3]);
                            if (!(value < GetValues("GB")))
                            {
                                producedHeat.Add(value - GetValues("GB")); // Subtract the maximum heat value for the unit
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (values[2] == $"OB+{unitname}")
                        {
                            double value = double.Parse(values[3]);
                            if (!(value < GetValues("OB")))
                            {
                                producedHeat.Add(value - GetValues("OB")); // Subtract the maximum heat value for the unit 
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (values[2] == $"EK+{unitname}")
                        {
                            double value = double.Parse(values[3]);
                            if (!(value < GetValues("EK")))
                            {
                                producedHeat.Add(value - GetValues("EK")); // Subtract the maximum heat value for the unit
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (values[2] == $"GM+{unitname}")
                        {
                            double value = double.Parse(values[3]);
                            if (!(value < GetValues("GM")))
                            {
                                producedHeat.Add(value - GetValues("EK")); // Subtract the maximum heat value for the unit
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            return producedHeat; // Return the list of produced heat values
        }

        // Graph logic
        public ISeries[] Series { get; set; } =
        {
            // Initialize the series with the produced heat data for each unit
            new StackedAreaSeries<double>
            {
                Values = ReadProducedHeat("Assets/ResultData.csv", "GB"), // Produced heat data for GB
                Fill = new SolidColorPaint(SKColors.Blue) // Set color for GB
            },
            new StackedAreaSeries<double>
            {
                Values = ReadProducedHeat("Assets/ResultData.csv", "OB"), // Produced heat data for OB
                Fill = new SolidColorPaint(SKColors.Orange) // Set color for OB
            },
            new StackedAreaSeries<double>
            {
                Values = ReadProducedHeat("Assets/ResultData.csv", "GM"), // Produced heat data for GM
                Fill = new SolidColorPaint(SKColors.Red) // Set color for GM
            },
            new StackedAreaSeries<double>
            {
                Values = ReadProducedHeat("Assets/ResultData.csv", "EK"), // Produced heat data for EK
                Fill = new SolidColorPaint(SKColors.Green) // Set color for EK
            }
        };

        // X and Y axes properties
        public Axis[] XAxes { get; set; } // X-axis
        public Axis[] YAxes { get; set; } // Y-axis

        // Constructor
        public VisualizerViewModel()
        {
            // Initialize X and Y axes
            XAxes = new Axis[]
            {
                new Axis
                {
                    CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
                    CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
                    CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
                    Labeler = value => value.ToString("N2") // Labeler function to format X-axis labels
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    CrosshairLabelsBackground = SKColors.DarkOrange.AsLvcColor(),
                    CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1),
                    CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1),
                    CrosshairSnapEnabled = true // Enable snapping for Y-axis
                }
            };
        }

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
