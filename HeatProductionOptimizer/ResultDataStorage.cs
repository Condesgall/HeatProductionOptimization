using System.Globalization;
using ResultDataManager_;

namespace ResultDataStorage{
    public interface IResultDataStorage
    {
        void Load();
        //void Save();
    }

    public class ResultDataCSV : IResultDataStorage
    {
        private string FilePath;
        public List<ResultDataManager> loadedResultData;
        

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
                        double.Parse(lineParts[3], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[4], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[5], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[6], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[7], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[8], CultureInfo.InvariantCulture),
                        double.Parse(lineParts[9], CultureInfo.InvariantCulture)
                        );

                    ResultDataManager currentData = new ResultDataManager(timeFrom, timeTo, unitName, results);
                    loadedResultData.Add(currentData);
                }
            }
        }

        /*
        public void Save()
        {
            using (var writer = new StreamWriter(FilePath))
            {
                // Write header line
                writer.WriteLine("UnitName,ProducedHeat,ProducedElectricity,ConsumedElectricity,Expenses,Profit,PrimaryEnergyConsumption,CO2Emissions");

                // Write data for each unit
                foreach (var unitResult in loadedResultData.resultData)
                {
                    string line = $"{unitResult.Key}," + 
                                $"{unitResult.Value.ProducedHeat}," + 
                                $"{unitResult.Value.ProducedElectricity}," +
                                $"{unitResult.Value.ConsumedElectricity}," +
                                $"{unitResult.Value.Expenses}," +
                                $"{unitResult.Value.Profit}," +
                                $"{unitResult.Value.PrimaryEnergyConsumption}," +
                                $"{unitResult.Value.Co2Emissions}";

                    writer.WriteLine(line);
                }
            }
        }*/
    }
}

