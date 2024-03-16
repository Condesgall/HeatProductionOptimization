using System;

public class SdmParameters
{
    public string TimeFrom {get; set;}
    public string TimeTo {get; set;}
    public string HeatDemand {get; set;}
    public string Price {get; set;}


    public void Load(string filePath, List<SdmParameters> summerList, List<SdmParameters> winterList)
    {
        using (var reader = new StreamReader(filePath))
        {
            // Skip the header line
            reader.ReadLine();

            // Process each line in the CSV file
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Split the line by comma
                string[] parts = line.Split(',');

                foreach(string part in parts)
                {
                    System.Console.Write($"{part}\t");
                }
                System.Console.WriteLine();
            }
        }
    }


    public SdmParameters(string timeFrom, string timeTo, double heatDemand, double price)
    {
        timeFrom = TimeFrom;
        timeTo = TimeTo;
        heatDemand = HeatDemand;
        price = Price;    
    }
}