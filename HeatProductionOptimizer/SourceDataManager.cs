using System;
using System.Globalization;

public class SdmParameters
{
    public string TimeFrom {get; set;}
    public string TimeTo {get; set;}
    public double HeatDemand {get; set;}
    public double ElPrice {get; set;}

    public SdmParameters(string timeFrom, string timeTo, double heatDemand, double elPrice)
    {
        TimeFrom = timeFrom;
        TimeTo = timeTo;
        HeatDemand = heatDemand;
        ElPrice = elPrice;
    }
}

public class ParameterLoader
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
        int i = 0;
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
                    // it takes "." as a decimal point, and not a separator as in some languages.
                    SdmParameters currentWinterParameters = new(
                        lineParts[0], 
                        lineParts[1], 
                        double.Parse(lineParts[2], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[3], CultureInfo.InvariantCulture));

                    SdmParameters currentSummerParameters = new(
                        lineParts[5],
                        lineParts[6],
                        double.Parse(lineParts[7], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[8], CultureInfo.InvariantCulture));

                    Winter.Add(currentWinterParameters);
                    Summer.Add(currentSummerParameters);

                    i++;
                }
            }
        }  
    }
}