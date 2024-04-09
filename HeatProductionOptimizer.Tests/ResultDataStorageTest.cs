using ResultDataManager_;
using ResultDataStorage;

namespace HeatProductionOptimizer.Tests
{
    class ResultDataStorageTest
    {
        static void ResultDataStorage_SaveDataToCSVWithValidProperties()
        {
            ResultDataManager resultDataManager = new ResultDataManager();

            resultDataManager.AddResultData("Unit1", new OptimizationResults(100.0, 200.0, 50.0, 500.0, 1000.0, 300.0, 50.0));
            resultDataManager.AddResultData("Unit2", new OptimizationResults(150.0, 250.0, 70.0, 600.0, 1200.0, 350.0, 60.0));

            ResultDataCSV resultDataCSV = new ResultDataCSV("test.csv");
            resultDataCSV.Save();

            Console.WriteLine("Data saved to test.csv");
        }
    }
}