using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

string filePath = "source_data.csv";
        List<decimal> electricityPrices = ReadElectricityPricesFromFile(filePath);

        // Analyze data
        if (electricityPrices.Any())
        {
            Console.WriteLine("Descriptive Statistics:");
            Console.WriteLine($"Mean: {CalculateMean(electricityPrices)}");
            Console.WriteLine($"Median: {CalculateMedian(electricityPrices)}");
            Console.WriteLine($"Standard Deviation: {CalculateStandardDeviation(electricityPrices)}");
            
        }
        else
        {
            Console.WriteLine("No data available.");
        }
    

    static List<decimal> ReadElectricityPricesFromFile(string filePath)
    {
        List<decimal> prices = new List<decimal>();
        try
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                // Skip header
                sr.ReadLine();
                
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 3 && decimal.TryParse(parts[2], out decimal price))
                    {
                        prices.Add(price);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
        }
        return prices;
    }

    static decimal CalculateMean(List<decimal> prices)
    {
        return prices.Average();
    }

    static decimal CalculateMedian(List<decimal> prices)
    {
        var sortedPrices = prices.OrderBy(x => x).ToList();
        int count = sortedPrices.Count;
        if (count % 2 == 0)
        {
            return (sortedPrices[count / 2 - 1] + sortedPrices[count / 2]) / 2;
        }
        else
        {
            return sortedPrices[count / 2];
        }
    }

    static decimal CalculateStandardDeviation(List<decimal> prices)
    {
        double mean = (double)CalculateMean(prices);
        double sumSquaredDifferences = prices.Sum(price => Math.Pow((double)price - mean, 2));
        double variance = sumSquaredDifferences / prices.Count;
        return (decimal)Math.Sqrt(variance);
    }
