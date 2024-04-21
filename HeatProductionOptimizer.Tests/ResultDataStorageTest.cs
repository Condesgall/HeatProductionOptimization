/*using Xunit;
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
            Assert.NotEmpty(resultDataCSV.loadedResultData);
            Assert.Contains("GB", resultDataCSV.loadedResultData[0].ProductionUnit);

            // Verify the values of loaded OptimizationResults
            OptimizationResults gbResults = resultDataCSV.loadedResultData[0].OptimizationResults;
            Assert.Equal(1, gbResults.ProducedHeat);
            Assert.Equal(2, gbResults.ProducedElectricity);
            Assert.Equal(3, gbResults.ConsumedElectricity);
            Assert.Equal(4, gbResults.Expenses);
            Assert.Equal(5, gbResults.Profit);
            Assert.Equal(6, gbResults.PrimaryEnergyConsumption);
            Assert.Equal(7, gbResults.Co2Emissions);
        }

        [Fact]
        public void Save_CorrectlyWritesDataToFile()
        {
            string testFilePath = Path.Combine(assemblyDirectory, "TestRDM.csv"); 
            OptimizationResults testData = new OptimizationResults(1.0m, 2.0m, 3.0m, 4.0m, 5.0m, 6.0m, 7.0m);
            ResultDataCSV resultDataCSV = new ResultDataCSV(testFilePath);

            ResultData testResultData = new ResultData("1/1/1970 0:00", "1/1/1970 1:00", "GB", testData);

            ResultDataManager.Winter.Add(testResultData);

            resultDataCSV.Save(ResultDataManager.Winter);

            // Assert
            // Read the saved file and verify its contents match the expected data
            using (var reader = new StreamReader(testFilePath))
            {
                // Skipping the first line
                reader.ReadLine();

                string line = reader.ReadLine();
                string[] lineParts = line.Split(',');

                Assert.Equal("1/1/1970 0:00", lineParts[0]);
                Assert.Equal("1/1/1970 1:00", lineParts[1]);
                Assert.Equal("GB", lineParts[2]);
                Assert.Equal("1.0", lineParts[3]);
                Assert.Equal("2.0", lineParts[4]);
                Assert.Equal("3.0", lineParts[5]);
                Assert.Equal("4.0", lineParts[6]);
                Assert.Equal("5.0", lineParts[7]);
                Assert.Equal("6.0", lineParts[8]);
                Assert.Equal("7.0", lineParts[9]);
            }
        }
    }
}*/