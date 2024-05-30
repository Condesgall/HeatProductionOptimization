using System.Globalization;
using HeatingGridAvaloniaApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HeatingGridAvaloniaApp.Models;

public class Optimizer
{
    public Dictionary<List<ProductionUnit>, decimal> individualUnitCandidates = new Dictionary<List<ProductionUnit>, decimal>();
    public Dictionary<ProductionUnit, decimal> unitPairingCandidates = new Dictionary<ProductionUnit, decimal>();
    public Dictionary<List<ProductionUnit>, decimal> combinedUnitsNetCost = new Dictionary<List<ProductionUnit>, decimal>();
    public List<Co2AndNetCost> co2AndNetCostsCandidates = new List<Co2AndNetCost>();

    private decimal netProductionCosts;
    private decimal netWeight;
    private decimal co2Weight;

    //properties
    public decimal NetProductionCosts
    {
        get { return netProductionCosts; }
        set { netProductionCosts = value; }
    }

    public decimal NetWeight
    {
        get { return netWeight; }
        set { netWeight = value; }
    }

    public decimal Co2Weight
    {
        get { return co2Weight; }
        set { co2Weight = value; }
    }


    public void OptimizeProduction(List<SdmParameters> sourceData, int optimizeBy)
    {
        ResultDataManager.ResultData.Clear();
        foreach (var sdmParameters in sourceData)
        {
            ProductionUnit primaryUnit;
            ProductionUnit secondaryUnit;
            // Check whether Oil or Gas Boiler is more efficient for given parameter.
            switch (optimizeBy)
            {
                //optimize cost only
                case 1:
                    if (AssetManager.productionUnits[0].ProductionCosts < AssetManager.productionUnits[1].ProductionCosts)
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
                    if (AssetManager.productionUnits[0].Co2Emissions < AssetManager.productionUnits[1].Co2Emissions)
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
                    decimal coefficient0 = (NetWeight * AssetManager.productionUnits[0].ProductionCosts) + (AssetManager.productionUnits[0].Co2Emissions * Co2Weight);
                    decimal coefficient1 = (NetWeight * AssetManager.productionUnits[1].ProductionCosts) + (AssetManager.productionUnits[1].Co2Emissions * Co2Weight);
                    if (coefficient0 < coefficient1)
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

            while (true)
            {
                // Empty resultData instance for results.
                ResultData resultData = new ResultData();

                // Declare currentMaxHeat with a value of unit's max heat.
                decimal currentMaxHeat = primaryUnit.MaxHeat;

                // Subtract currentHeatNeeded from currentMaxHeat. If the result is 
                currentMaxHeat -= currentHeatNeeded;

                // Check if current unit meets the whole demand.
                if (currentMaxHeat >= 0)
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

    public void SaveToResultDataManager(ResultData resultData, SdmParameters sdmParameters)
    {
        // Check the source data and add result data to the right list.
        ResultDataManager.ResultData.Add(resultData);
    }

    /*_____________________________________________________________________________________________________________
                                                   SCENARIO 2
   _______________________________________________________________________________________________________________*/

    public void OptimizeResultsSc2(List<SdmParameters> sourceData, int optimizeBy)
    {
        ResultDataManager.ResultData.Clear();
        foreach (var sdmParameters in sourceData)
        {
            switch (optimizeBy)
            {
                //optimize by costs
                case 1:
                    OptimizeByCostsHandler(sdmParameters);
                    break;
                case 2:
                    OptimizeByCO2EmissionHandler(sdmParameters);
                    break;
                case 3:
                    OptimizeByCO2AndCostsHandler(sdmParameters);
                    break;
                default:
                    Console.WriteLine("Please select an option.");
                    break;
            }
        }
    }

    public void OptimizeByCostsHandler(SdmParameters sdmParameters)
    {
        Dictionary<List<ProductionUnit>, decimal> netCostResults = GetOptimizedNetCosts(sdmParameters);
        List<ResultData> topResults = new List<ResultData>();

        foreach (var result in netCostResults)
        {
            ProductionUnit optimalUnit = result.Key.First();
            ProductionUnit optimalUnit2;

            if (result.Key.Count == 1)
            {
                optimalUnit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
            }
            else
            {
                optimalUnit2 = result.Key.Last();
            }

            decimal netCost = result.Value;
            ResultData newResultData = new ResultData();
            newResultData.UpdateResultData(optimalUnit, optimalUnit2, netCost, sdmParameters);

            topResults.Add(newResultData);

            if (topResults.Count() >= 1)
            {
                break;
            }
        }

        foreach (var result in topResults)
        {
            SaveToResultDataManager(result, sdmParameters);
        }
    }

    public void OptimizeByCO2EmissionHandler(SdmParameters sdmParameters)
    {
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        ProductionUnit unit1 = new ProductionUnit("", 0, 0, 0, 0, 0);
        List<Co2AndNetCost> optimizedResults = GetOptimizedCO2(sdmParameters);
        List<ResultData> topResults = new List<ResultData>();

        foreach (var result in optimizedResults)
        {
            if (result.ProductionUnits != null)
            {
                unit1 = result.ProductionUnits.First();

                // if it's a combination of units
                if (result.ProductionUnits.Count() == 2)
                {
                    unit2 = result.ProductionUnits.Last();
                }
                else
                {
                    unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
                }
            }
            ResultData newResultData = new ResultData();
            newResultData.UpdateResultData(unit1, unit2, result.NetCost, sdmParameters);

            topResults.Add(newResultData);

            if (topResults.Count() >= 1)
            {
                break;
            }
        }
        foreach (var result in topResults)
        {
            SaveToResultDataManager(result, sdmParameters);
        }
    }

    public void OptimizeByCO2AndCostsHandler(SdmParameters sdmParameters)
    {
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        ProductionUnit unit1 = new ProductionUnit("", 0, 0, 0, 0, 0);
        List<Co2AndNetCost> optimizedResults = GetOptimizedCo2AndNet(sdmParameters, AssetManager.productionUnits);
        List<ResultData> topResults = new List<ResultData>();

        foreach (var result in optimizedResults)
        {
            if (result.ProductionUnits != null)
            {
                unit1 = result.ProductionUnits.First();

                // if it's a combination of units
                if (result.ProductionUnits.Count() == 2)
                {
                    unit2 = result.ProductionUnits.Last();
                }
                else
                {
                    unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
                }
            }

            ResultData newResultData = new ResultData();
            newResultData.UpdateResultData(unit1, unit2, result.NetCost, sdmParameters);

            topResults.Add(newResultData);

            if (topResults.Count() >= 1)
            {
                break;
            }
        }
        foreach (var result in topResults)
        {
            SaveToResultDataManager(result, sdmParameters);
        }
    }

    public Dictionary<List<ProductionUnit>, decimal> GetOptimizedNetCosts(SdmParameters sdmParameters)
    {
        GroupUnitsByDependency(sdmParameters, AssetManager.productionUnits);

        // best combinations of two units
        GetBestUnitCombinations(sdmParameters, 0, 1); //combinedUnitsNetCosts
        var result = individualUnitCandidates
            .Concat(combinedUnitsNetCost)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        result = result.OrderBy(key => key.Value).ToDictionary(x => x.Key, x => x.Value);
        return result;
    }

    public List<Co2AndNetCost> GetOptimizedCO2(SdmParameters sdmParameters)
    {
        GroupUnitsByDependency(sdmParameters, AssetManager.productionUnits);
        CalculateCo2IndividualUnits(sdmParameters);
        GetBestUnitCombinations(sdmParameters, 0, 1);
        CalculateCo2CombinedUnits(sdmParameters);
        co2AndNetCostsCandidates = co2AndNetCostsCandidates.OrderBy(unit => unit.Co2Emissions).ToList();

        return co2AndNetCostsCandidates;
    }

    public List<Co2AndNetCost> GetOptimizedCo2AndNet(SdmParameters sdmParameters, List<ProductionUnit> productionUnits)
    {
        GroupUnitsByDependency(sdmParameters, productionUnits);
        GetBestUnitCombinations(sdmParameters, 0, 1);

        CalculateCo2IndividualUnits(sdmParameters);
        CalculateCo2CombinedUnits(sdmParameters);

        List<Co2AndNetCost> co2AndNetCostsResults = new List<Co2AndNetCost>();

        foreach (var units in co2AndNetCostsCandidates)
        {
            if (!units.HaveCo2Emissions())
            {
                units.Co2Emissions = 1;
            }
            decimal result = (NetWeight * units.NetCost) + (Co2Weight * units.Co2Emissions);
            units.Result = result;
            co2AndNetCostsResults.Add(units);
        }
        co2AndNetCostsResults = co2AndNetCostsResults.OrderBy(unit => unit.Result).ToList();
        return co2AndNetCostsResults;
    }


    /*--------------------------------------------------------------------------------
        METHODS RELATED TO INDIVIDUAL UNIT CALCULATIONS
    ---------------------------------------------------------------------------------*/

    public void GroupUnitsByDependency(SdmParameters sdmParameters, List<ProductionUnit> productionUnits)
    {
        foreach (var productionUnit in productionUnits)
        {
            decimal netCost = CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);

            if (productionUnit.CanReachHeatDemand(sdmParameters))
            {
                List<ProductionUnit> units = new List<ProductionUnit>() { productionUnit };
                individualUnitCandidates.Add(units, netCost);
            }
            //checks if dictionary contains that value already
            if (!unitPairingCandidates.ContainsKey(productionUnit))
            {
                unitPairingCandidates.Add(productionUnit, netCost);
            }
        }
        //orders the values (ascending)
        individualUnitCandidates = individualUnitCandidates.OrderBy(key => key.Value).ToDictionary(x => x.Key, x => x.Value);
        unitPairingCandidates = unitPairingCandidates.OrderBy(key => key.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    public void CalculateCo2IndividualUnits(SdmParameters sdmParameters)
    {
        foreach (var units in individualUnitCandidates.Keys)
        {
            foreach (var productionUnit in units)
            {
                decimal co2Emissions = sdmParameters.HeatDemand * productionUnit.Co2Emissions;
                decimal netCost = individualUnitCandidates[units];
                List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit };

                Co2AndNetCost co2AndNetCostOfUnit = new Co2AndNetCost(productionUnits, netCost, co2Emissions, 0);
                co2AndNetCostsCandidates.Add(co2AndNetCostOfUnit);
            }
        }
    }

    public decimal CalculateIndividualUnitNetCosts(SdmParameters sdmParameters, ProductionUnit productionUnit)
    {
        // for electricity producing units
        if (productionUnit.GetProductionUnitType() == -1)
        {
            NetCostsForElProducingUnitsHandler(productionUnit, sdmParameters);
        }
        // for electricity consuming units
        else if (productionUnit.GetProductionUnitType() == -2)
        {
            NetCostsForElConsumingUnitsHandler(productionUnit, sdmParameters);
        }
        // for heat only boilers
        else if (productionUnit.GetProductionUnitType() == -3)
        {
            NetCostsForHeatOnlyUnitsHandler(productionUnit, sdmParameters);
        }
        return NetProductionCosts;
    }

    public void NetCostsForElProducingUnitsHandler(ProductionUnit productionUnit, SdmParameters sdmParameters)
    {
        decimal expenses;
        decimal profit;
        decimal electricityProduced = productionUnit.CalculateElectricityProduced(sdmParameters.HeatDemand);

        if (productionUnit.CanReachHeatDemand(sdmParameters))
        {
            profit = electricityProduced * sdmParameters.ElPrice;
            expenses = sdmParameters.HeatDemand * productionUnit.ProductionCosts;
            NetProductionCosts = expenses - profit;
        }
        else
        {
            expenses = productionUnit.MaxHeat * productionUnit.ProductionCosts;
            profit = productionUnit.MaxElectricity * sdmParameters.ElPrice;
            NetProductionCosts = expenses - profit;
        }
    }

    public void NetCostsForElConsumingUnitsHandler(ProductionUnit productionUnit, SdmParameters sdmParameters)
    {
        decimal expenses;
        decimal extraExpenses;

        if (productionUnit.CanReachHeatDemand(sdmParameters))
        {
            expenses = sdmParameters.HeatDemand * productionUnit.ProductionCosts;
            extraExpenses = sdmParameters.HeatDemand * sdmParameters.ElPrice;
            NetProductionCosts = expenses + extraExpenses;
        }
        else
        {
            expenses = productionUnit.MaxHeat * productionUnit.ProductionCosts;
            extraExpenses = (-productionUnit.MaxElectricity) * sdmParameters.ElPrice;
            NetProductionCosts = expenses + extraExpenses;
        }
    }

    public void NetCostsForHeatOnlyUnitsHandler(ProductionUnit productionUnit, SdmParameters sdmParameters)
    {
        if (productionUnit.CanReachHeatDemand(sdmParameters))
        {
            NetProductionCosts = productionUnit.ProductionCosts * sdmParameters.HeatDemand;
        }
        else
        {
            NetProductionCosts = productionUnit.ProductionCosts * productionUnit.MaxHeat;
        }
    }

    /*------------------------------------------------------------------------------
        METHODS RELATED TO UNIT COMBINATIONS AND SELECTION
    --------------------------------------------------------------------------------*/

    public Dictionary<List<ProductionUnit>, decimal> GetBestUnitCombinations(SdmParameters sdmParameters, int primaryUnitIndex, int secondUnitIndex)
    {
        List<ProductionUnit> unitCandidates = unitPairingCandidates.Keys.ToList();

        if (primaryUnitIndex < unitCandidates.Count)
        {
            if (secondUnitIndex < unitCandidates.Count)
            {
                if (primaryUnitIndex != secondUnitIndex)
                {
                    ProductionUnit optimalUnit = unitCandidates[primaryUnitIndex];
                    ProductionUnit optimalUnit2 = unitCandidates[secondUnitIndex];

                    if (ProductionUnit.CombinedUnitsReachHeatDemand(sdmParameters, optimalUnit, optimalUnit2))
                    {
                        AddCombinationToDictionary(optimalUnit, optimalUnit2);
                    }
                    // Recursively call the method with updated indices
                    return GetBestUnitCombinations(sdmParameters, primaryUnitIndex, secondUnitIndex + 1);
                }
                else
                {
                    return GetBestUnitCombinations(sdmParameters, primaryUnitIndex, secondUnitIndex + 1);
                }
            }
            else
            {
                // Move to the next unit when secondUnitIndex exceeds bounds
                return GetBestUnitCombinations(sdmParameters, primaryUnitIndex + 1, 0);
            }
        }
        combinedUnitsNetCost = combinedUnitsNetCost.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        return combinedUnitsNetCost;
    }

    public void AddCombinationToDictionary(ProductionUnit optimalUnit, ProductionUnit unit2)
    {
        List<ProductionUnit> options = new List<ProductionUnit> { optimalUnit, unit2 };


        decimal netCost1 = unitPairingCandidates[optimalUnit];
        decimal netCost2 = unitPairingCandidates[unit2];

        decimal totalNetCost = netCost1 + netCost2;

        // Add to unit options if it doesn't already exist
        if (!combinedUnitsNetCost.ContainsKey(options))
        {
            combinedUnitsNetCost.Add(options, totalNetCost);
        }
    }

    public void CalculateCo2CombinedUnits(SdmParameters sdmParameters)
    {
        foreach (var unitCombination in combinedUnitsNetCost.Keys)
        {
            ProductionUnit productionUnit1 = unitCombination.First();
            ProductionUnit productionUnit2 = unitCombination.Last();

            decimal co2Emissions_Unit1 = productionUnit1.MaxHeat * productionUnit1.Co2Emissions;
            decimal co2Emissions_Unit2 = (sdmParameters.HeatDemand - productionUnit1.MaxHeat) * productionUnit2.Co2Emissions;

            decimal netCost = combinedUnitsNetCost[unitCombination];
            decimal totalCo2Emissions = co2Emissions_Unit1 + co2Emissions_Unit2;

            Co2AndNetCost co2AndNetCostOfCombination = new Co2AndNetCost(unitCombination, netCost, totalCo2Emissions, 0);
            co2AndNetCostsCandidates.Add(co2AndNetCostOfCombination);
        }
    }
}


public class Co2AndNetCost
{
    private decimal netCost;
    private decimal co2Emissions;
    private List<ProductionUnit>? productionUnits;
    private decimal result;

    public Co2AndNetCost(List<ProductionUnit> productionUnits_, decimal netCost_, decimal co2Emissions_, decimal result_)
    {
        productionUnits = productionUnits_;
        netCost = netCost_;
        co2Emissions = co2Emissions_;
    }

    public decimal NetCost
    {
        get { return netCost; }
        set { netCost = value; }
    }

    public decimal Co2Emissions
    {
        get { return co2Emissions; }
        set { co2Emissions = value; }
    }

    public List<ProductionUnit>? ProductionUnits
    {
        get { return productionUnits; }
        set { productionUnits = value; }
    }

    public decimal Result
    {
        get { return result; }
        set { result = value; }
    }

    public bool HaveCo2Emissions()
    {
        if (Co2Emissions == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}