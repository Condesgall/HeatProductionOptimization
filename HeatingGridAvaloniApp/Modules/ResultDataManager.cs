using System.Globalization;
using HeatingGridAvaloniaApp.Modules;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System;
using System.Collections.Generic;

namespace HeatingGridAvaloniaApp.Modules

{
public class OptimizationResults
{
    public decimal ProducedHeat { get; set; }
    public decimal ProducedElectricity { get; set; }
    public decimal ConsumedElectricity { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
    public decimal PrimaryEnergyConsumption { get; set; }
    public decimal Co2Emissions { get; set; }

    // Constructor
    public OptimizationResults(decimal producedHeat, decimal producedElectricity, decimal consumedElectricity, decimal expenses, decimal profit, decimal primaryEnergyConsumption, decimal co2Emissions)
    {
        ProducedHeat = producedHeat;
        ProducedElectricity = producedElectricity;
        ConsumedElectricity = consumedElectricity;
        Expenses = expenses;
        Profit = profit;
        PrimaryEnergyConsumption = primaryEnergyConsumption;
        Co2Emissions = co2Emissions;
    }
}

    public class ResultData
    {
        public string TimeFrom;
        public string TimeTo;
        public string ProductionUnit;
        public OptimizationResults OptimizationResults = new OptimizationResults(0,0,0,0,0,0,0);

        // Parameterless constructor for Optimizer.OptimizeProduction();
        public ResultData()
        {
            TimeFrom = "";
            TimeTo = "";
            ProductionUnit = "";
            OptimizationResults = new OptimizationResults(0,0,0,0,0,0,0);
        }

        public ResultData(string timeFrom, string timeTo, string productionUnit, OptimizationResults optimizationResults)
        {
            TimeFrom = timeFrom;
            TimeTo = timeTo;
            ProductionUnit = productionUnit;
            OptimizationResults = optimizationResults;
        }
    }

    public class ResultDataManager
    {
        Optimizer optimizer = new Optimizer();
        public static List<ResultData> Winter = new List<ResultData>();
        public static List<ResultData> Summer = new List<ResultData>();

        public void GetFilePathAndUpdateLists()
        {
            if (File.Exists("SourceData.csv"))
            {
                // Get the full file path
                string fullPath = Path.GetFullPath("SourceData.csv");
                ParameterLoader parameterLoader = new ParameterLoader(fullPath);
                parameterLoader.Load();
                optimizer.OptimizeProduction(parameterLoader.Summer, 2);
                optimizer.OptimizeProduction(parameterLoader.Winter, 2);
            }
        }

        public void DisplayResultData(List<ResultData> list)
        {
            GetFilePathAndUpdateLists();
            foreach (var resultData in list)
            {
                Console.WriteLine($"Time: {resultData.TimeFrom}-{resultData.TimeTo}");
                Console.WriteLine($"Production unit: {resultData.ProductionUnit}");
                Console.WriteLine("");
                Console.WriteLine($"Optimization results:");
                Console.WriteLine("");
                Console.WriteLine($"Produced heat: {resultData.OptimizationResults.ProducedHeat} MW");
                Console.WriteLine($"Produced electricity: {resultData.OptimizationResults.ProducedElectricity} MW");
                Console.WriteLine($"Consumed electricity: {resultData.OptimizationResults.ConsumedElectricity} MW");
                Console.WriteLine($"Expenses: {resultData.OptimizationResults.Expenses} DKK");
                Console.WriteLine($"Primary energy consumption: {resultData.OptimizationResults.PrimaryEnergyConsumption} MWh");
                Console.WriteLine($"CO2 emissions: {resultData.OptimizationResults.Co2Emissions} kg");
                Console.WriteLine("_____________________________");
            }
        }
    }

}