using System.Globalization;
using HeatingGridAvaloniaApp.Modules;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatingGridAvaloniaApp.Modules;

public class Optimizer
{
    public  Dictionary<ProductionUnit, decimal> individualUnitsNetCosts = new Dictionary<ProductionUnit, decimal>();
    public  Dictionary<List<ProductionUnit>, decimal> unitsAndNetCosts = new Dictionary<List<ProductionUnit>, decimal>();
    public  List<decimal> netCostValues = new List<decimal>();
    public Dictionary<ProductionUnit, decimal> individualUnitsOrdered = new Dictionary<ProductionUnit, decimal>();
    
    private decimal netProductionCosts;

    //properties
    public decimal GetNetProductionCosts
    {
        get { return netProductionCosts; }
        set { netProductionCosts = value; }
    }
    

    public void OptimizeProduction(List<SdmParameters> sourceData, int optimizeBy)
    {
        foreach (var sdmParameters in sourceData)
        {
            ProductionUnit primaryUnit;
            ProductionUnit secondaryUnit;
            // Check whether Oil or Gas Boiler is more efficient for given parameter.
            switch(optimizeBy)
            {
                //optimize cost only
                case 1:
                if(AssetManager.productionUnits[0].ProductionCosts<AssetManager.productionUnits[1].ProductionCosts)
                {
                    primaryUnit = AssetManager.productionUnits[0];
                    secondaryUnit = AssetManager.productionUnits[1];
                }
                else
                {
                    primaryUnit = AssetManager.productionUnits[1];
                    secondaryUnit = AssetManager.productionUnits[0];
                }
                break;
                
                //optimize co2 only
                case 2:
                if(AssetManager.productionUnits[0].Co2Emissions<AssetManager.productionUnits[1].Co2Emissions)
                {
                    primaryUnit = AssetManager.productionUnits[0];
                    secondaryUnit = AssetManager.productionUnits[1];
                }
                else
                {
                    primaryUnit = AssetManager.productionUnits[1];
                    secondaryUnit = AssetManager.productionUnits[0];
                }
                break;

                //optimize co2 and costs
                case 3:
                // This checks expenses * co2 emissions, i.e. rate of both cost
                // and co2 emissions when producing a MWh of heat.
                decimal coefficient0 = AssetManager.productionUnits[0].ProductionCosts * AssetManager.productionUnits[0].Co2Emissions;
                decimal coefficient1 = AssetManager.productionUnits[1].ProductionCosts * AssetManager.productionUnits[1].Co2Emissions;
                if(coefficient0<coefficient1)
                {
                    primaryUnit = AssetManager.productionUnits[0];
                    secondaryUnit = AssetManager.productionUnits[1];
                }
                else
                {
                    primaryUnit = AssetManager.productionUnits[1];
                    secondaryUnit = AssetManager.productionUnits[0];
                }
                break;

                default: 
                System.Console.WriteLine("Incorrect optimizeBy parameter.");
                return;
            }

            // Check how much heat is needed.
            decimal currentHeatNeeded = sdmParameters.HeatDemand;

            while(true)
            {
                // Empty resultData instance for results.
                ResultData resultData = new ResultData();

                // Declare currentMaxHeat with a value of unit's max heat.
                decimal currentMaxHeat = primaryUnit.MaxHeat;

                // Subtract currentHeatNeeded from currentMaxHeat. If the result is 
                currentMaxHeat -= currentHeatNeeded;

                // Check if current unit meets the whole demand.
                if(currentMaxHeat >= 0)
                {
                    // *Produce* all heat with the first boiler.
                    resultData.OptimizationResults.ProducedHeat = currentHeatNeeded;
                    resultData.OptimizationResults.Expenses = currentHeatNeeded * primaryUnit.ProductionCosts;
                    resultData.OptimizationResults.PrimaryEnergyConsumption = currentHeatNeeded * primaryUnit.GasConsumption;
                    resultData.OptimizationResults.Co2Emissions = currentHeatNeeded * primaryUnit.Co2Emissions;
                    resultData.TimeFrom = sdmParameters.TimeFrom;
                    resultData.TimeTo = sdmParameters.TimeTo;
                    resultData.ProductionUnit = primaryUnit.Name;

                    // Save the data.
                    SaveToResultDataManager(resultData, sdmParameters);

                    break;
                }
                else
                {
                    // *Produce* as much heat as primaryUnit can.
                    resultData.OptimizationResults.ProducedHeat = primaryUnit.MaxHeat;
                    resultData.OptimizationResults.Expenses = primaryUnit.MaxHeat * primaryUnit.ProductionCosts;
                    resultData.OptimizationResults.PrimaryEnergyConsumption = currentHeatNeeded * primaryUnit.GasConsumption;
                    resultData.OptimizationResults.Co2Emissions = primaryUnit.MaxHeat * primaryUnit.Co2Emissions;
                    resultData.TimeFrom = sdmParameters.TimeFrom;
                    resultData.TimeTo = sdmParameters.TimeTo;
                    resultData.ProductionUnit = primaryUnit.Name;

                    // Then let secondaryUnit produce the rest.
                    currentHeatNeeded = currentMaxHeat * (-1.0m);
                    primaryUnit = secondaryUnit;

                    // Save the data.
                    SaveToResultDataManager(resultData, sdmParameters);
                }
            }  
        }
    }

    public string GetSeason(SdmParameters sdmParameters)
    {
        // In given data set, summer has values around 1.5 and winter has values around 6.5
        if(sdmParameters.HeatDemand > 4) return "winter";
        else return "summer";
    }

    public void SaveToResultDataManager(ResultData resultData, SdmParameters sdmParameters)
    {
        // Check the source data and add result data to the right list.
        if(GetSeason(sdmParameters)=="winter") ResultDataManager.Winter.Add(resultData);
        if(GetSeason(sdmParameters)=="summer") ResultDataManager.Summer.Add(resultData);
    }
    
    /*_____________________________________________________________________________________________________________
                                                    SCENARIO 2
    _______________________________________________________________________________________________________________*/

    public decimal CalculateNetProductionCosts(SdmParameters sdmParameters, ProductionUnit productionUnit)
    {
        //production unit produces electricity
        if (productionUnit.MaxElectricity > 0)
        {
            //if electricity that can be produced isnt over the electricity limit
            if (sdmParameters.HeatDemand <= productionUnit.MaxElectricity)
            {
                netProductionCosts = sdmParameters.HeatDemand * (productionUnit.ProductionCosts - sdmParameters.ElPrice);
            }
            //if it is over the electricity limit
            if (sdmParameters.HeatDemand > productionUnit.MaxElectricity)
            {
                netProductionCosts = (sdmParameters.HeatDemand * productionUnit.ProductionCosts) - (productionUnit.MaxElectricity * sdmParameters.ElPrice);
            }
            //if the heat demand is bigger than the max heat
            if (sdmParameters.HeatDemand > productionUnit.MaxHeat)
            {
                netProductionCosts = productionUnit.MaxHeat * productionUnit.ProductionCosts - productionUnit.MaxElectricity * sdmParameters.ElPrice;
            }
        }
        //production unit consumes electricity
        else if (productionUnit.MaxElectricity < 0)
        {
            //if heat demand doesn't surpass the heat limit
            if (sdmParameters.HeatDemand <= productionUnit.MaxHeat)
            {
                netProductionCosts = sdmParameters.HeatDemand * (productionUnit.ProductionCosts + sdmParameters.ElPrice);
            }
            else
            {
                netProductionCosts = productionUnit.MaxHeat * (productionUnit.ProductionCosts + sdmParameters.ElPrice);
            }
        }
        //heat only
        else if (productionUnit.MaxElectricity == 0)
        {
            //if heat demand doesnt surpass the heat limit
            if (sdmParameters.HeatDemand <= productionUnit.MaxHeat)
            {
                netProductionCosts = productionUnit.ProductionCosts*sdmParameters.HeatDemand;
            }
            else
            {
                netProductionCosts = productionUnit.ProductionCosts*productionUnit.MaxHeat;
            }
        }
        return netProductionCosts;
    }
    public Dictionary<ProductionUnit, decimal> GetProductionUnitsNetCosts(SdmParameters sdmParameters, List<ProductionUnit> productionUnits)
    {
        foreach (var productionUnit in productionUnits)
        {
            decimal netCost = CalculateNetProductionCosts(sdmParameters, productionUnit);
            //adds net cost and corresponding production unit to dictionary
            individualUnitsNetCosts.Add(productionUnit, netCost);
        }
        //orders the values (ascending)
        individualUnitsNetCosts.OrderBy(key => key.Value);
        foreach (var unit in individualUnitsNetCosts)
        {
            Console.WriteLine(unit.Key.Name);
            Console.WriteLine(unit.Value);
        }
        return individualUnitsNetCosts;
    }
    public void OptimizeResultsSc2(List<SdmParameters> sourceData, int optimizeBy)
    {
        ResultData resultData = new ResultData();
        foreach (var sdmParameters in sourceData)
        {
            //dictionary that contains all individual unit net costs (by order)
            individualUnitsOrdered = GetProductionUnitsNetCosts(sdmParameters, AssetManager.productionUnits);
            //list of production units in order (by net costs)
            List<ProductionUnit> unitSortedList = new List<ProductionUnit> (individualUnitsOrdered.Keys);

            //dictionary that contains the units that meet the heat demand
            Dictionary<ProductionUnit, decimal> unitsThatMeetDemand = new Dictionary<ProductionUnit, decimal>();

            foreach (var individualUnit in individualUnitsOrdered)
            {
                if (sdmParameters.HeatDemand <= individualUnit.Key.MaxHeat)
                {
                    unitsThatMeetDemand.Add(individualUnit.Key, individualUnit.Value);
                }
            }

            switch (optimizeBy)
            {
                //optimize by costs
                case 1:
                    OptimizeByCostsHandler(resultData, sdmParameters, unitSortedList, unitsThatMeetDemand);
                break;

                default:
                break;
            }
        }
    }

    public void OptimizeByCostsHandler(ResultData resultData, SdmParameters sdmParameters, List<ProductionUnit> unitSortedList, Dictionary<ProductionUnit, decimal> unitsThatMeetDemand)
    {
        decimal heatDemand = sdmParameters.HeatDemand;
        List<ProductionUnit> optionsList = new List<ProductionUnit>();
        //gets unit combinations and their net costs. Example unit 1 and unit 2, and their net cost (added together)
        Dictionary<List<ProductionUnit>, decimal> unitCombinations = NetCostsWhenMoreThan1Unit(sdmParameters, heatDemand, resultData, unitSortedList, optionsList, 0, 1);
        //creates a list of the net costs (from unit combinations)
        List<decimal> unitCombinationsNetCosts = unitCombinations.Values.ToList();
        //creates a list of the net costs (individual units)
        List<decimal> individualUnitsNetCosts = unitsThatMeetDemand.Values.ToList();

        List<decimal> allNetCosts = new List<decimal> (unitCombinationsNetCosts);
        //adds combinations of units net costs and individual units net costs
        allNetCosts.AddRange(individualUnitsNetCosts);
        //orders the values by ascending order
        allNetCosts.OrderBy(x => x);

        //checks if the most optimal net cost is from an individual unit
        if (individualUnitsNetCosts.Contains(allNetCosts[0]))
        {
            ProductionUnit optimalUnit = individualUnitsOrdered.First().Key;
            ProductionUnit optimalUnit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
            UpdateResultData(resultData, optimalUnit, optimalUnit2, allNetCosts[0], heatDemand);
        }
        //or a combination of 2 units
        else if (unitCombinationsNetCosts.Contains(allNetCosts[0]))
        {
            List<ProductionUnit> firstCombination = unitCombinations.Keys.First();
            ProductionUnit optimalUnit = firstCombination[0];
            ProductionUnit optimalUnit2 = firstCombination[1];
            UpdateResultData(resultData, optimalUnit, optimalUnit2, allNetCosts[0], heatDemand);
        }
        SaveToResultDataManager(resultData, sdmParameters);
    }

    public Dictionary<List<ProductionUnit>, decimal> NetCostsWhenMoreThan1Unit(SdmParameters sdmParameters, decimal heatDemand, ResultData resultData, List<ProductionUnit> unitSortedList, List<ProductionUnit> optionsList, int index, int index2)
    {
        if (index < unitSortedList.Count)
        {
            if (index2 < unitSortedList.Count)
            {
                ProductionUnit optimalUnit2 = unitSortedList[index2];
                ProductionUnit optimalUnit = unitSortedList[index];

                // If the heat demand is met
                if (sdmParameters.HeatDemand <= optimalUnit.MaxHeat + optimalUnit2.MaxHeat)
                {
                    optionsList.Clear();
                    optionsList.Add(optimalUnit);
                    optionsList.Add(optimalUnit2);
                    
                    // Calculate individual net costs
                    decimal netCosts1 = CalculateNetProductionCosts(sdmParameters, optimalUnit);
                    decimal netCosts2 = CalculateNetProductionCosts(sdmParameters, optimalUnit2);
                    
                    // Calculate total net cost
                    decimal totalNetCost = netCosts1 + netCosts2;
                    
                    // Add to unit options
                    if (!unitsAndNetCosts.ContainsKey(optionsList))
                    {
                        unitsAndNetCosts.Add(optionsList, totalNetCost);
                    }
                }
                // Recursively call the method with updated indices
                NetCostsWhenMoreThan1Unit(sdmParameters, heatDemand, resultData, unitSortedList, optionsList, index, index2 + 1);
            }
            else
            {
                // Move to the next index when index2 exceeds bounds
                NetCostsWhenMoreThan1Unit(sdmParameters, heatDemand, resultData, unitSortedList, optionsList, index + 1, index + 2);
            }
        }
        return unitsAndNetCosts;
    }

    public void UpdateResultData(ResultData resultData, ProductionUnit optimalUnit, ProductionUnit optimalUnit2, decimal netCost, decimal heatDemand)
    {
        resultData.OptimizationResults.ProducedHeat = heatDemand;
        if (netCost > 0)
        {
            resultData.OptimizationResults.Profit = netCost;
        }
        else
        {
            resultData.OptimizationResults.Expenses = netCost;
        }

        //if the gas consumption is 0, that means the electric boiler is being used
        if (optimalUnit.GasConsumption == 0)
        {
            //if the electric boiler is being used, no other production unit is being used, since the electric boiler always reaches the heat demand.
            resultData.OptimizationResults.PrimaryEnergyConsumption = heatDemand;
        }
        else
        {
            resultData.OptimizationResults.PrimaryEnergyConsumption = heatDemand * (optimalUnit.GasConsumption + optimalUnit2.GasConsumption);
        }
        //if the heat demand is less than the first unit max heat
        if (heatDemand <= optimalUnit.MaxHeat)
        {
            resultData.OptimizationResults.Co2Emissions = heatDemand * optimalUnit.Co2Emissions;
        }
        else //if the heat demand is greater than the first unit's max heat
        {
            resultData.OptimizationResults.Co2Emissions = optimalUnit.MaxHeat * optimalUnit.Co2Emissions + (heatDemand - optimalUnit.MaxHeat) * optimalUnit2.Co2Emissions;
        }

        if (optimalUnit2.Name != "")
        {
            resultData.ProductionUnit = optimalUnit.Name + "," + optimalUnit2.Name;
        }
        else
        {
            resultData.ProductionUnit = optimalUnit.Name;
        }
    }
}