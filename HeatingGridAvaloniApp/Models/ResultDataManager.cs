using System.Globalization;
using HeatingGridAvaloniaApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatingGridAvaloniaApp.Models

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
        private string? timeFrom;
        private string? timeTo;
        private string? productionUnit;

        public string? TimeFrom
        {
            get { return timeFrom; }
            set { timeFrom = value; }
        }

        public string? TimeTo
        {
            get { return timeTo; }
            set { timeTo = value; }
        }

        public string? ProductionUnit
        {
            get { return productionUnit; }
            set { productionUnit = value; }
        }

        public OptimizationResults OptimizationResults { get; set; }

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
            if (netCost < 0)
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
                ProductionUnit = optimalUnit.Name + "+" + secondUnit.Name;
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
                        OptimizationResults.ProducedElectricity = totalElectricity;
                    }
                    else
                    {
                        OptimizationResults.ConsumedElectricity = totalElectricity;
                    }
                    break;

                // El producing + heat only
                case -2:
                    OptimizationResults.ProducedElectricity = optimalUnit.CalculateElectricityProduced(heatDemand);
                    break;

                // heat only + el producing
                case -3:
                    OptimizationResults.ProducedElectricity = secondUnit.CalculateElectricityProduced(heatRemaining);
                    break;

                // heat only + el consuming
                case -4:
                    OptimizationResults.ConsumedElectricity = secondUnit.CalculateElectricityConsumed(heatRemaining);
                    break;

                // el producing
                case -5:
                    OptimizationResults.ProducedElectricity = optimalUnit.CalculateElectricityProduced(heatDemand);
                    break;
                
                // el consuming
                case -6:
                    OptimizationResults.ConsumedElectricity = optimalUnit.CalculateElectricityConsumed(heatDemand);
                    break;

                default:
                    OptimizationResults.ProducedElectricity = 0;
                    OptimizationResults.ConsumedElectricity = 0;
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
        public static List<ResultData> ResultData = new List<ResultData>();
        private decimal averageNetCost;
        private decimal averageCo2;
        public decimal AverageNetCost
        {
            get
            {
                if (averageNetCost == 0)
                {
                    averageNetCost = GetAverageNetCost();
                }
                return averageNetCost;
            }
            set
            {
                averageNetCost = value;
            }
        }

        public decimal AverageCo2
        {
            get
            {
                if (averageCo2 == 0)
                {
                    averageCo2 = GetAverageCo2();
                }
                return averageCo2;
            }
            set
            {
                averageCo2 = value;
            }
        }
        
        public decimal GetAverageNetCost()
        {
            List<decimal> netCosts = new List<decimal>();
            foreach (var resultData in ResultData)
            {
                decimal profit = resultData.OptimizationResults.Profit;
                decimal expenses = resultData.OptimizationResults.Expenses;
                if (profit == 0)
                {
                    netCosts.Add(expenses);
                }
                else if (expenses == 0)
                {
                    netCosts.Add(profit);
                }
            }
            return netCosts.Average();
        }

        public decimal GetAverageCo2()
        {
            List<decimal> co2 = new List<decimal>();
            foreach (var resultData in ResultData)
            {
                decimal co2Emissions = resultData.OptimizationResults.Co2Emissions;
                co2.Add(co2Emissions);
            }
            return co2.Average();
        }
    }
}