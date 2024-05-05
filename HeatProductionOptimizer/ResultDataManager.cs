using System.Globalization;
using AssetManager_;

namespace ResultDataManager_
{
public class OptimizationResults
{
    private decimal producedHeat;
    private decimal producedElectricity;
    private decimal consumedElectricity;
    private decimal expenses;
    private decimal profit;
    private decimal primaryEnergyConsumption;
    private decimal co2Emissions;

    public decimal ProducedHeat 
    { 
        get { return producedHeat; }
        set { producedHeat = value; }
    }

    public decimal ProducedElectricity
    { 
        get { return producedElectricity; }
        set { producedElectricity = value; }
    }

    public decimal ConsumedElectricity
    { 
        get { return consumedElectricity; }
        set { consumedElectricity = value; }
    }

    public decimal Expenses
    { 
        get { return expenses; }
        set { expenses = value; }
    }

    public decimal Profit
    { 
        get { return profit; }
        set { profit = value; }
    }

    public decimal PrimaryEnergyConsumption
    { 
        get { return primaryEnergyConsumption; }
        set { primaryEnergyConsumption = value; }
    }

    public decimal Co2Emissions
    { 
        get { return co2Emissions; }
        set { co2Emissions = value; }
    }

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
        private string timeFrom;
        private string timeTo;
        private string productionUnit;
        private OptimizationResults optimizationResults;
        
        public string TimeFrom
        {
            get { return timeFrom; }
            set { timeFrom = value; }
        }

        public string TimeTo
        {
            get { return timeTo; }
            set { timeTo = value; }
        }

        public string ProductionUnit
        {
            get { return productionUnit; }
            set { productionUnit = value; }
        }

        public OptimizationResults OptimizationResults
        {
            get { return optimizationResults; }
            set { optimizationResults = value; }
        }

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

        public void UpdateResultData(ProductionUnit optimalUnit, ProductionUnit secondUnit, decimal netCost, SdmParameters sdmParameters)
        {
            UpdateResultData_Name(optimalUnit, secondUnit);
            timeFrom = sdmParameters.TimeFrom;
            timeTo = sdmParameters.TimeTo;

            decimal heatDemand = sdmParameters.HeatDemand;
            OptimizationResults.ProducedHeat = heatDemand;
            UpdateOptimizationResults_NetCosts(netCost);
            UpdateOptimizationResults_PrimaryEnergyConsumption(optimalUnit, secondUnit, sdmParameters);
            UpdateOptimizationResults_Co2Emissions(optimalUnit, secondUnit, sdmParameters);
            UpdateOptimizationResults_Electricity(optimalUnit, secondUnit, sdmParameters);
        }

        public void UpdateOptimizationResults_NetCosts(decimal netCost)
        {
            if (netCost > 0)
            {
                OptimizationResults.Profit = netCost;
            }
            else
            {
                OptimizationResults.Expenses = netCost;
            }
        }

        public void UpdateOptimizationResults_PrimaryEnergyConsumption(ProductionUnit optimalUnit, ProductionUnit secondUnit, SdmParameters sdmParameters)
        {
            decimal heatDemand = sdmParameters.HeatDemand;
            if (optimalUnit.IsElectricBoiler())
            {
                //no other production unit is being used, since the electric boiler always reaches the heat demand.
                OptimizationResults.PrimaryEnergyConsumption = heatDemand;
            }
            else
            {
                UpdateOptimizationResults_Electricity(optimalUnit, secondUnit, sdmParameters);

                decimal electricityConsumed = OptimizationResults.ConsumedElectricity;
                decimal gasConsumption = optimalUnit.MaxHeat * optimalUnit.GasConsumption + (heatDemand - optimalUnit.MaxHeat) * secondUnit.GasConsumption;

                if (PrimaryEnergyIsGas(gasConsumption, electricityConsumed))
                {
                    OptimizationResults.PrimaryEnergyConsumption = gasConsumption;
                }
                else
                {
                    OptimizationResults.PrimaryEnergyConsumption = OptimizationResults.ConsumedElectricity;
                }
            }
        }

        public void UpdateOptimizationResults_Co2Emissions(ProductionUnit optimalUnit, ProductionUnit secondUnit, SdmParameters sdmParameters)
        {
            decimal heatDemand = sdmParameters.HeatDemand;
            if (optimalUnit.CanReachHeatDemand(sdmParameters))
            {
                OptimizationResults.Co2Emissions = heatDemand * optimalUnit.Co2Emissions;
            }
            else
            {
                OptimizationResults.Co2Emissions = 
                optimalUnit.MaxHeat * optimalUnit.Co2Emissions + (heatDemand - optimalUnit.MaxHeat) * secondUnit.Co2Emissions;
            }
        }

        public void UpdateResultData_Name(ProductionUnit optimalUnit, ProductionUnit secondUnit)
        {
            if (secondUnit.IfThereIsASecondUnit())
            {
                ProductionUnit = optimalUnit.Name + ", " + secondUnit.Name;
            }
            else
            {
                ProductionUnit = optimalUnit.Name;
            }
        }

        public void UpdateOptimizationResults_Electricity(ProductionUnit optimalUnit, ProductionUnit secondUnit, SdmParameters sdmParameters)
        {
            int typeOfcombination = CheckWhatTypeOfCombination(optimalUnit, secondUnit);
            decimal heatDemand = sdmParameters.HeatDemand;
            decimal heatRemaining = heatDemand - optimalUnit.MaxHeat;

            switch (typeOfcombination)
            {
                // El producing unit + el consuming unit
                case -1:
                    
                    decimal electricityProduced = optimalUnit.CalculateElectricityProduced(heatDemand);
                    decimal electricityConsumed = secondUnit.CalculateElectricityConsumed(heatRemaining);

                    decimal totalElectricity = electricityProduced - electricityConsumed;
                    if (totalElectricity > 0)
                    {
                        optimizationResults.ProducedElectricity = totalElectricity;
                    }
                    else
                    {
                        optimizationResults.ConsumedElectricity = totalElectricity;
                    }
                    break;

                // El producing + heat only
                case -2:
                    optimizationResults.ProducedElectricity = optimalUnit.CalculateElectricityProduced(heatDemand);
                    break;

                // heat only + el producing
                case -3:
                    optimizationResults.ProducedElectricity = secondUnit.CalculateElectricityProduced(heatRemaining);
                    break;

                // heat only + el consuming
                case -4:
                    optimizationResults.ConsumedElectricity = secondUnit.CalculateElectricityConsumed(heatRemaining);
                    break;

                // el producing
                case -5:
                    optimizationResults.ProducedElectricity = optimalUnit.CalculateElectricityProduced(heatDemand);
                    break;
                
                // el consuming
                case -6:
                    optimizationResults.ConsumedElectricity = optimalUnit.CalculateElectricityConsumed(heatDemand);
                    break;

                default:
                    optimizationResults.ProducedElectricity = 0;
                    optimizationResults.ConsumedElectricity = 0;
                    break;
            }
        }

        public int CheckWhatTypeOfCombination(ProductionUnit optimalUnit, ProductionUnit secondUnit)
        {
            int optimalUnitType = optimalUnit.GetProductionUnitType();
            int secondUnitType = secondUnit.GetProductionUnitType();

            // Electricity producing unit + Electricity consuming unit
            if (optimalUnitType == -1 && secondUnitType == -2)
            {
                return -1;
            }
            // Electricity producing unit + Heat boiler
            else if (optimalUnitType == -1 && secondUnitType == -3)
            {
                return -2;
            }
            // Heat boiler + Electricity producing unit
            else if (optimalUnitType == -3 && secondUnitType == -1)
            {
                return -3;
            }
            // Heat boiler + Electricity consuming unit
            else if (optimalUnitType == -3 && secondUnitType == -2)
            {
                return -4;
            }
            // Electricity producing unit only
            else if (optimalUnitType == -1 && secondUnitType == -4)
            {
                return -5;
            }
            // Electricity consuming unit only
            else if (optimalUnitType == -2 && secondUnitType == -4)
            {
                return -6;
            }
            // No valid combination found
            return 0;
        }

        public bool ElecricityProducing(decimal electricityProduced, decimal electricityConsumed)
        {
            if (electricityProduced - electricityConsumed >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PrimaryEnergyIsGas(decimal gasConsumption, decimal consumedElectricity)
        {
            if (gasConsumption > consumedElectricity)
            {
                return true;
            }
            else
            {
                return false;
            }
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


        // Time filter is declared within function's arguments to make it optional.
        public void DisplayResultData(List<ResultData> list, string optionalTimeFrom = "1/1/2000 00:00", string optionalTimeTo = "12/31/2100 23:59")
        {
            // Convert provided dates to DateTime format.
            string dateFormat = "M/d/yyyy H:m";
            DateTime searchedTimeFrom = DateTime.ParseExact(optionalTimeFrom, dateFormat, CultureInfo.InvariantCulture);
            DateTime searchedTimeTo = DateTime.ParseExact(optionalTimeTo, dateFormat, CultureInfo.InvariantCulture);

            GetFilePathAndUpdateLists();

            foreach (var resultData in list)
            {
                // Convert resultData dates to DateTime format.
                DateTime timeFrom = DateTime.ParseExact(resultData.TimeFrom, dateFormat, CultureInfo.InvariantCulture);
                DateTime timeTo = DateTime.ParseExact(resultData.TimeTo, dateFormat, CultureInfo.InvariantCulture);

                // Check if resultData you're on is in the scope of searched dates. If yes, proceed.
                if(timeFrom >= searchedTimeFrom && timeTo <= searchedTimeTo)
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

}