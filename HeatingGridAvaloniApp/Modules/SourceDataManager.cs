using System;
using System.Globalization;
using AssetManager_;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;

namespace HeatingGidAvaloniaApp.Modules;

public interface IFileLoading
{
    void Load();
    void DisplayWinterData();
    void DisplaySummerData();
}

public class SdmParameters
{
    public string TimeFrom {get; set;}
    public string TimeTo {get; set;}
    public decimal HeatDemand;
    public decimal ElPrice {get; set;}

    public SdmParameters(string timeFrom, string timeTo, decimal heatDemand, decimal elPrice)
    {
        TimeFrom = timeFrom;
        TimeTo = timeTo;
        HeatDemand = heatDemand;
        ElPrice = elPrice;
    }
}

public class ParameterLoader : IFileLoading
{
    public List<SdmParameters> Winter { get; private set; }
    public List<SdmParameters> Summer { get; private set; }
    
    private string FilePath;

    public ParameterLoader(string filePath)
    {
        Winter = new List<SdmParameters>();
        Summer = new List<SdmParameters>();
        FilePath = filePath;
    }

    public void Load()
    {
        using (var reader = new StreamReader(FilePath))
        {
            // Going line by line, reading all the parameters from each.
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineParts = line.Split(',');
                
                // Skipping the start of the file and other possibly unnecessary fields.
                if(!lineParts[0].Contains("/")) 
                {
                    continue;
                }
                else
                {
                    // Inputting the read data into temporary objects (the InvariantCulture part is to make sure
                    // it takes "." as a decimal point, instead of "," as in some languages).
                    SdmParameters currentWinterParameters = new(
                        lineParts[0], 
                        lineParts[1], 
                        decimal.Parse(lineParts[2], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[3], CultureInfo.InvariantCulture));

                    SdmParameters currentSummerParameters = new(
                        lineParts[5],
                        lineParts[6],
                        decimal.Parse(lineParts[7], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[8], CultureInfo.InvariantCulture));

                    Winter.Add(currentWinterParameters);
                    Summer.Add(currentSummerParameters);
                }
            }
        }  
    }

    public void DisplaySummerData()
    {
        Console.WriteLine("\n\n\n\t -----------\n\t|SUMMER DATA|\n\t -----------");
        Console.WriteLine();
        foreach(SdmParameters param in Summer)
        {
            Console.WriteLine($"Time from: {param.TimeFrom}");
            Console.WriteLine($"Time to: {param.TimeTo}");
            Console.WriteLine($"Heat demand: {param.HeatDemand}");
            Console.WriteLine($"Electricity price: {param.ElPrice}");
            Console.WriteLine();
        }
    }

    public void DisplayWinterData()
    {
        Console.WriteLine("\t -----------\n\t|WINTER DATA|\n\t -----------");
        Console.WriteLine();
        foreach(SdmParameters param in Winter)
        {
            Console.WriteLine($"Time from: {param.TimeFrom}");
            Console.WriteLine($"Time to: {param.TimeTo}");
            Console.WriteLine($"Heat demand: {param.HeatDemand}");
            Console.WriteLine($"Electricity price: {param.ElPrice}");
            Console.WriteLine();
        }
    }
}