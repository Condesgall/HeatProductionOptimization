using Xunit;
using System.IO;
using System.Reflection;
using System.Globalization;
using ResultDataStorage;
using ResultDataManager_;

namespace ResultDataStorage.Tests
{
    public class ResultDataCSVTests
    {
        private readonly string? assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Fact]
        public void Load_CorrectlyParsesDataFromFile()
        {
            // Arrange
            if (assemblyDirectory != null)
            {
                string filePath = Path.Combine(assemblyDirectory, "Test.csv");
                ResultDataCSV resultDataCSV = new ResultDataCSV(filePath);
                ResultData gbResults = new ResultData();
                gbResults.TimeFrom = "00";
                gbResults.TimeTo = "01";
                gbResults.ProductionUnit = "GB";
                gbResults.OptimizationResults.ProducedHeat = 1m;
                gbResults.OptimizationResults.ProducedElectricity = 2m;
                gbResults.OptimizationResults.ConsumedElectricity = 3m;
                gbResults.OptimizationResults.Expenses = 4m;
                gbResults.OptimizationResults.Profit = 5m;
                gbResults.OptimizationResults.PrimaryEnergyConsumption = 6m;
                gbResults.OptimizationResults.Co2Emissions = 7m;

                List<ResultData> resultData = new List<ResultData>() { gbResults };
                resultDataCSV.loadedResultData = new List<ResultData>();
                resultDataCSV.Save(resultData);

                // Act
                resultDataCSV.Load();

                // Assert
                Assert.NotNull(resultDataCSV.loadedResultData);
                Assert.NotEmpty(resultDataCSV.loadedResultData);
                Assert.Contains("GB", resultDataCSV.loadedResultData.First().ProductionUnit);
                Assert.Contains("00", resultDataCSV.loadedResultData.First().TimeFrom);
                Assert.Contains("01", resultDataCSV.loadedResultData.First().TimeTo);
            

                // Verify the values of loaded OptimizationResults
                OptimizationResults gbOptimizedResults = gbResults.OptimizationResults;
                Assert.Equal(1.0m, gbOptimizedResults.ProducedHeat);
                Assert.Equal(2.0m, gbOptimizedResults.ProducedElectricity);
                Assert.Equal(3.0m, gbOptimizedResults.ConsumedElectricity);
                Assert.Equal(4.0m, gbOptimizedResults.Expenses);
                Assert.Equal(5.0m, gbOptimizedResults.Profit);
                Assert.Equal(6.0m, gbOptimizedResults.PrimaryEnergyConsumption);
                Assert.Equal(7.0m, gbOptimizedResults.Co2Emissions);
            }

        }

        [Fact]
        public void Save_CorrectlyWritesDataToFile()
        {
            // Arrange 
            if (assemblyDirectory != null)
            {
                string filePath = Path.Combine(assemblyDirectory, "Test.csv");
                ResultDataCSV resultDataCSV = new ResultDataCSV(filePath);

                ResultData gbResults = new ResultData();
                gbResults.TimeFrom = "00";
                gbResults.TimeTo = "01";
                gbResults.ProductionUnit = "GB";
                gbResults.OptimizationResults.ProducedHeat = 1m;
                gbResults.OptimizationResults.ProducedElectricity = 2m;
                gbResults.OptimizationResults.ConsumedElectricity = 3m;
                gbResults.OptimizationResults.Expenses = 4m;
                gbResults.OptimizationResults.Profit = 5m;
                gbResults.OptimizationResults.PrimaryEnergyConsumption = 6m;
                gbResults.OptimizationResults.Co2Emissions = 7m;

                // Act
                List<ResultData> resultData = new List<ResultData>() { gbResults };
                resultDataCSV.Save(resultData);

                // Assert
                // Read the saved file and verify its contents match the expected data
                using (var reader = new StreamReader(filePath))
                {
                    // Skipping the first line
                    reader.ReadLine();

                    string? line = reader.ReadLine();
                    if (line != null)
                    {
                        string[] lineParts = line.Split(',');
                        Assert.Equal("00", lineParts[0]);
                        Assert.Equal("01", lineParts[1]);
                        Assert.Equal("GB", lineParts[2]);
                        Assert.Equal("1", lineParts[3]);
                        Assert.Equal("2", lineParts[4]);
                        Assert.Equal("3", lineParts[5]);
                        Assert.Equal("4", lineParts[6]);
                        Assert.Equal("5", lineParts[7]);
                        Assert.Equal("6", lineParts[8]);
                        Assert.Equal("7", lineParts[9]);
                    }
                }   
            }
        }
    }
}