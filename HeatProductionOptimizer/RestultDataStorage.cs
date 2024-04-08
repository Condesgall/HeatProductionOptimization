using System.Globalization;
using ResultDataManager_;

public interface IResultDataStorage
{
    void Load();
    void Save();
}

public class ResultDataCSV : IResultDataStorage
{
    private string FilePath;
    ResultDataManager loadedResultData = new ResultDataManager();
    

    public ResultDataCSV(string filePath)
    {
        FilePath = filePath;
    }

    public void Load()
    {
        using (var reader = new StreamReader(FilePath))
        {
            // Skipping the first line
            reader.ReadLine();

            // Going line by line, reading all the parameters from each.
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] lineParts = line.Split(',');
            
                // Loading everything part by part, considering first column to be unitName, and following columns are result data parameters
                string unitName = lineParts[0];
                OptimizationResults results = new(
                    double.Parse(lineParts[1], CultureInfo.InvariantCulture),
                    double.Parse(lineParts[2], CultureInfo.InvariantCulture),
                    double.Parse(lineParts[3], CultureInfo.InvariantCulture),
                    double.Parse(lineParts[4], CultureInfo.InvariantCulture),
                    double.Parse(lineParts[5], CultureInfo.InvariantCulture),
                    double.Parse(lineParts[6], CultureInfo.InvariantCulture),
                    double.Parse(lineParts[7], CultureInfo.InvariantCulture)
                    );

                loadedResultData.AddResultData(unitName, results);
            }
        }
    }

    public void Save()
    {
        // To do.   
    }
}

