using System.Globalization;
using ResultDataManager_;

namespace ResultDataStorage{
    public interface IResultDataStorage
    {
        void Load();
        void Save(List<ResultData> rdm);
    }

    public class ResultDataCSV : IResultDataStorage
    {
        private string FilePath;
        public List<ResultData>? loadedResultData;

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
                    string timeFrom = lineParts[0];
                    string timeTo = lineParts[1];
                    string unitName = lineParts[2];
                    OptimizationResults results = new(
                        decimal.Parse(lineParts[3], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[4], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[5], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[6], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[7], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[8], CultureInfo.InvariantCulture),
                        decimal.Parse(lineParts[9], CultureInfo.InvariantCulture)
                        );

                    ResultData currentData = new ResultData(timeFrom, timeTo, unitName, results);
                    if (loadedResultData != null) 
                    {
                        loadedResultData.Add(currentData);
                    }
                }
            }
        }

        public void Save(List<ResultData> rdm)
        {
            using (var writer = new StreamWriter(FilePath))
            {
                //header
                writer.WriteLine("TimeFrom,TimeTo,UnitName,ProducedHeat,ProducedElectricity,ConsumedElectricity,Expenses,Profit,PrimaryEnergyConsumption,CO2Emissions");

                // Loop that writes data for each object in the rdm
                foreach (var resultData in rdm)
                {
                    // Construct the data line witch the data from objects
                    string line = $"{resultData.TimeFrom}," +
                                  $"{resultData.TimeTo}," +
                                  $"{resultData.ProductionUnit}," +
                                  $"{resultData.OptimizationResults.ProducedHeat}," +
                                  $"{resultData.OptimizationResults.ProducedElectricity}," +
                                  $"{resultData.OptimizationResults.ConsumedElectricity}," +
                                  $"{resultData.OptimizationResults.Expenses}," +
                                  $"{resultData.OptimizationResults.Profit}," +
                                  $"{resultData.OptimizationResults.PrimaryEnergyConsumption}," +
                                  $"{resultData.OptimizationResults.Co2Emissions}";

                    // Write the line to the file
                    writer.WriteLine(line);
                }

            }
        }
    }
}

