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
        private readonly string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [Fact]
        public void Load_CorrectlyParsesDataFromFile()
        {
            string filePath = Path.Combine(assemblyDirectory, "Test.csv");
            ResultDataCSV resultDataCSV = new ResultDataCSV(filePath);

            resultDataCSV.Load();

            // Assert
            Assert.NotNull(resultDataCSV.loadedResultData);
            Assert.NotEmpty(resultDataCSV.loadedResultData.resultData);
            Assert.Contains("GB", resultDataCSV.loadedResultData.resultData.Keys);

            // Verify the values of loaded OptimizationResults
            OptimizationResults gbResults = resultDataCSV.loadedResultData.resultData["GB"];
            Assert.Equal(1.0, gbResults.ProducedHeat);
            Assert.Equal(2.0, gbResults.ProducedElectricity);
            Assert.Equal(3.0, gbResults.ConsumedElectricity);
            Assert.Equal(4.0, gbResults.Expenses);
            Assert.Equal(5.0, gbResults.Profit);
            Assert.Equal(6.0, gbResults.PrimaryEnergyConsumption);
            Assert.Equal(7.0, gbResults.Co2Emissions);
        }

        [Fact]
        public void Save_CorrectlyWritesDataToFile()
        {
            string testFilePath = Path.Combine(assemblyDirectory, "Test.csv"); 
            var resultDataManager = new ResultDataManager();
            OptimizationResults testData = new OptimizationResults(1, 2, 3, 4, 5, 6, 7);
            resultDataManager.AddResultData("GB", testData);
            ResultDataCSV resultDataCSV = new ResultDataCSV(testFilePath);

            resultDataCSV.loadedResultData = resultDataManager; 
            resultDataCSV.Save();

            // Assert
            // Read the saved file and verify its contents match the expected data
            using (var reader = new StreamReader(testFilePath))
            {
                // Skipping the first line
                reader.ReadLine();

                string line = reader.ReadLine();
                string[] lineParts = line.Split(',');

                Assert.Equal("GB", lineParts[0]);
                Assert.Equal("1", lineParts[1]);
                Assert.Equal("2", lineParts[2]);
                Assert.Equal("3", lineParts[3]);
                Assert.Equal("4", lineParts[4]);
                Assert.Equal("5", lineParts[5]);
                Assert.Equal("6", lineParts[6]);
                Assert.Equal("7", lineParts[7]);
            }
        }
    }
}