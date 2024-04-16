namespace ResultDataManager_
{
public class OptimizationResults
{
    public double ProducedHeat { get; set; }
    public double ProducedElectricity { get; set; }
    public double ConsumedElectricity { get; set; }
    public double Expenses { get; set; }
    public double Profit { get; set; }
    public double PrimaryEnergyConsumption { get; set; }
    public double Co2Emissions { get; set; }

    // Constructor
    public OptimizationResults(double producedHeat, double producedElectricity, double consumedElectricity, double expenses, double profit, double primaryEnergyConsumption, double co2Emissions)
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
                optimizer.OptimizeProduction(parameterLoader.Summer);
                optimizer.OptimizeProduction(parameterLoader.Winter);
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
                Console.WriteLine($"Produced heat: {resultData.OptimizationResults.ProducedHeat}");
                Console.WriteLine($"Produced electricity: {resultData.OptimizationResults.ProducedElectricity}");
                Console.WriteLine($"Consumed electricity: {resultData.OptimizationResults.ConsumedElectricity}");
                Console.WriteLine($"Expenses: {resultData.OptimizationResults.Expenses}");
                Console.WriteLine($"Primary energy consumption: {resultData.OptimizationResults.PrimaryEnergyConsumption}");
                Console.WriteLine($"CO2 emissions: {resultData.OptimizationResults.Co2Emissions}");
                Console.WriteLine("_____________________________");
            }
        }
    }

}