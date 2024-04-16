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
        public static List<ResultData> Winter = new List<ResultData>();
        public static List<ResultData> Summer = new List<ResultData>();
    }
}