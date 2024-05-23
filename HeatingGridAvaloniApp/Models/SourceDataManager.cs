using System;
using System.Globalization;
using HeatingGridAvaloniaApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Collections.Generic;

namespace HeatingGridAvaloniaApp.Models;

public interface IFileLoading
{
    void Load();
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
    public List<SdmParameters> SDMParameters { get; private set; }
    
    private string FilePath;

    public ParameterLoader(string filePath)
    {
        SDMParameters = new List<SdmParameters>();
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

                    SDMParameters.Add(currentWinterParameters);
                    SDMParameters.Add(currentSummerParameters);
                }
            }
        }  
    }
}