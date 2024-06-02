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
    public List<ProductionUnit>individualUnitCandidates = new List<ProductionUnit>();
    public List<ProductionUnit> unitPairingCandidates = new List<ProductionUnit>();
    public SortedSet<Co2AndNetCost> unitCandidates = new SortedSet<Co2AndNetCost>(new Co2AndNetCostComparer());

    private decimal netProductionCosts;
    //properties
    public decimal NetProductionCosts
    {
        get { return netProductionCosts; }
        set { netProductionCosts = value; }
    }


    public void OptimizeProduction(List<SdmParameters> sourceData, int optimizeBy, decimal netWeight, decimal co2Weight)
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
                    decimal coefficient0 = (netWeight * AssetManager.productionUnits[0].ProductionCosts) + (AssetManager.productionUnits[0].Co2Emissions * co2Weight);
                    decimal coefficient1 = (netWeight * AssetManager.productionUnits[1].ProductionCosts) + (AssetManager.productionUnits[1].Co2Emissions * co2Weight);
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

            // Empty resultData instance for results.
            ResultData resultData = new ResultData();
            while (true)
            {
                // Declare currentMaxHeat with a value of unit's max heat.
                decimal currentMaxHeat = primaryUnit.MaxHeat;

                // Subtract currentHeatNeeded from currentMaxHeat. If the result is 
                currentMaxHeat -= currentHeatNeeded;

                // Check if current unit meets the whole demand.
                if (currentMaxHeat >= 0)
                {
                    // *Produce* all heat with the first boiler.
                    resultData.OptimizationResults.ProducedHeat += currentHeatNeeded;
                    resultData.OptimizationResults.Expenses += currentHeatNeeded * primaryUnit.ProductionCosts;
                    resultData.OptimizationResults.PrimaryEnergyConsumption += currentHeatNeeded * primaryUnit.GasConsumption;
                    resultData.OptimizationResults.Co2Emissions += currentHeatNeeded * primaryUnit.Co2Emissions;
                    resultData.TimeFrom = sdmParameters.TimeFrom;
                    resultData.TimeTo = sdmParameters.TimeTo;

                    // If there was more heat produced before, add the unit name to the string with unit names
                    // else, only use add the unit name without the '+' sign before it
                    if(resultData.ProductionUnit=="") resultData.ProductionUnit = primaryUnit.Name;
                    else resultData.ProductionUnit += $"+{primaryUnit.Name}";

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

    public void OptimizeResultsSc2(List<SdmParameters> sourceData, int optimizeBy, decimal netWeight, decimal co2Weight)
    {
        ResultDataManager.ResultData.Clear();
        SortedSet<Co2AndNetCost> sortedUnits;
        foreach (var sdmParameters in sourceData)
        {
            switch (optimizeBy)
            {
                //optimize by costs
                case 1:
                    sortedUnits = GetOptimizedNetCosts(sdmParameters, netWeight, co2Weight);
                    SaveResult(sdmParameters, sortedUnits);
                    break;
                case 2:
                    sortedUnits = GetOptimizedCO2(sdmParameters, netWeight, co2Weight);
                    SaveResult(sdmParameters, sortedUnits);
                    break;
                case 3:
                    sortedUnits = GetOptimizedCo2AndNet(sdmParameters, netWeight, co2Weight);
                    SaveResult(sdmParameters, sortedUnits);
                    break;
                default:
                    Console.WriteLine("Please select an option.");
                    break;
            }
        }
    }

    public void SaveResult(SdmParameters sdmParameters, SortedSet<Co2AndNetCost> sortedUnits)
    {
        Co2AndNetCost bestCandidate = sortedUnits.First();      
        
        ResultData newResultData = new ResultData();
        newResultData.UpdateResultData(bestCandidate, sdmParameters);

        SaveToResultDataManager(newResultData, sdmParameters);
    }

    public SortedSet<Co2AndNetCost> GetOptimizedNetCosts(SdmParameters sdmParameters, decimal netWeight, decimal co2Weight)
    {
        GetOptimalUnits(sdmParameters, netWeight, co2Weight);
        SortedSet<Co2AndNetCost> sortedUnitCandidates = new SortedSet<Co2AndNetCost>(unitCandidates, Comparer<Co2AndNetCost>.Create((x, y)
        => x.NetCost.CompareTo(y.NetCost)));
        return sortedUnitCandidates;
    }

    public SortedSet<Co2AndNetCost> GetOptimizedCO2(SdmParameters sdmParameters, decimal netWeight, decimal co2Weight)
    {
        GetOptimalUnits(sdmParameters, netWeight, co2Weight);
        SortedSet<Co2AndNetCost> sortedUnitCandidates = new SortedSet<Co2AndNetCost>(unitCandidates, Comparer<Co2AndNetCost>.Create((x, y)
        => x.Co2Emissions.CompareTo(y.Co2Emissions)));
        return sortedUnitCandidates;
    }

    public SortedSet<Co2AndNetCost> GetOptimizedCo2AndNet(SdmParameters sdmParameters, decimal netWeight, decimal co2Weight)
    {
        GetOptimalUnits(sdmParameters, netWeight, co2Weight);
        SortedSet<Co2AndNetCost> sortedUnitCandidates = new SortedSet<Co2AndNetCost>(unitCandidates, Comparer<Co2AndNetCost>.Create((x, y)
        => x.Result.CompareTo(y.Result)));
        return sortedUnitCandidates;
    }

    public void GetOptimalUnits(SdmParameters sdmParameters, decimal netWeight, decimal co2Weight)
    {
        unitCandidates.Clear();
        GroupUnitsByDependency(sdmParameters, AssetManager.productionUnits);

        var uniqueUnits = new HashSet<string>();
        foreach (var unit in individualUnitCandidates)
        {
            List<ProductionUnit> units = new List<ProductionUnit>() { unit };
            string unitKey = unit.Name;

            if (!uniqueUnits.Contains(unitKey))
            {
                uniqueUnits.Add(unitKey);
                CalculateCo2AndNet(sdmParameters, units, netWeight, co2Weight);
            }
        }
        GetBestUnitCombinations(sdmParameters, 0, 1, netWeight, co2Weight);
    }

    public void GroupUnitsByDependency(SdmParameters sdmParameters, List<ProductionUnit> productionUnits)
    {        
        individualUnitCandidates.Clear();
        unitPairingCandidates.Clear();
        foreach (var productionUnit in productionUnits)
        {
            if (productionUnit.CanReachHeatDemand(sdmParameters))
            {
                individualUnitCandidates.Add(productionUnit);
            }
            if (!unitPairingCandidates.Contains(productionUnit))
            {
                unitPairingCandidates.Add(productionUnit);
            }
        }
    }

    public void CalculateCo2AndNet(SdmParameters sdmParameters, List<ProductionUnit> productionUnits, decimal netWeight, decimal co2Weight)
    {
        decimal totalNetCost = 0;
        decimal totalCo2 = 0;
        if (productionUnits.Count() == 2)
        {
            //Console.WriteLine($"{productionUnits.First().Name}+{productionUnits.Last().Name}");
            //Console.WriteLine("Passed into unit combinations calculations");
            CalculateCo2AndNetUnitCombination(sdmParameters, productionUnits, ref totalCo2, ref totalNetCost);

            decimal result = (co2Weight * totalCo2) + (totalNetCost * netWeight);
            Co2AndNetCost co2AndNetCostOfUnit = new Co2AndNetCost(productionUnits, totalNetCost, totalCo2, result);

            if(!unitCandidates.Contains(co2AndNetCostOfUnit))
            {
                unitCandidates.Add(co2AndNetCostOfUnit);
            }
        }
        else
        {
            CalculateCo2AndNetIndividualUnits(sdmParameters, productionUnits, ref totalCo2, ref totalNetCost);

            decimal result = (co2Weight * totalCo2) + (totalNetCost * netWeight);
            Co2AndNetCost co2AndNetCostOfUnit = new Co2AndNetCost(productionUnits, totalNetCost, totalCo2, result);

            if (!unitCandidates.Contains(co2AndNetCostOfUnit))
            {
                unitCandidates.Add(co2AndNetCostOfUnit);
            }
            
        }
    }

    public void CalculateCo2AndNetUnitCombination(SdmParameters sdmParameters, List<ProductionUnit> productionUnits, ref decimal totalCo2, ref decimal totalNetCost)
    {
        decimal heatProducedUnit1 = productionUnits.First().MaxHeat;
        decimal heatProducedUnit2 = sdmParameters.HeatDemand - heatProducedUnit1;

        decimal co2EmissionsUnit1 = heatProducedUnit1 * productionUnits.First().Co2Emissions;
        decimal co2EmissionsUnit2 = heatProducedUnit2 * productionUnits.Last().Co2Emissions;
        totalCo2 = co2EmissionsUnit1 + co2EmissionsUnit2;

        totalNetCost = CalculateIndividualUnitNetCosts(sdmParameters, productionUnits, heatProducedUnit1);
    }

    public void CalculateCo2AndNetIndividualUnits(SdmParameters sdmParameters, List<ProductionUnit> productionUnits, ref decimal totalCo2, ref decimal totalNetCost)
    {
        decimal heatProducedUnit1 = sdmParameters.HeatDemand;
        totalCo2 = heatProducedUnit1 * productionUnits.First().Co2Emissions;
        totalNetCost = CalculateIndividualUnitNetCosts(sdmParameters, productionUnits, heatProducedUnit1);
    }

    public decimal CalculateIndividualUnitNetCosts(SdmParameters sdmParameters, List<ProductionUnit> productionUnits, decimal heatProduced)
    {
        List<decimal> netCosts = new List<decimal>();

        // for electricity producing units
        foreach (var productionUnit in productionUnits)
        {
            if (productionUnit.GetProductionUnitType() == -1)
            {
                NetCostsForElProducingUnitsHandler(productionUnits, sdmParameters, heatProduced);
            }
            // for electricity consuming units
            else if (productionUnit.GetProductionUnitType() == -2)
            {
                NetCostsForElConsumingUnitsHandler(productionUnits, sdmParameters, heatProduced);
            }
            // for heat only boilers
            else if (productionUnit.GetProductionUnitType() == -3)
            {
                NetCostsForHeatOnlyUnitsHandler(productionUnits, sdmParameters, heatProduced);
            }
            netCosts.Add(NetProductionCosts);
        }
        if (productionUnits.Count() == 2)
        {
            return netCosts.Sum();
        }
        else
        {
            return NetProductionCosts;
        }
    }

    public void NetCostsForElProducingUnitsHandler(List<ProductionUnit> productionUnits, SdmParameters sdmParameters, decimal heatProduced)
    {
        decimal expenses;
        decimal profit;

        if (productionUnits.First().GetProductionUnitType() == -1)
        {
            ProductionUnit productionUnit = productionUnits.First();
            decimal electricityProduced = productionUnit.CalculateElectricityProduced(heatProduced);

            if (productionUnit.CanReachHeatDemand(sdmParameters))
            {
                expenses = heatProduced * productionUnit.ProductionCosts;
                profit = electricityProduced * sdmParameters.ElPrice;
                NetProductionCosts = expenses - profit;
            }
            else
            {
                expenses = productionUnit.MaxHeat * productionUnit.ProductionCosts;
                profit = productionUnit.MaxElectricity * sdmParameters.ElPrice;
                NetProductionCosts = expenses - profit;
            }
        }
        else if(productionUnits.Last().GetProductionUnitType() == -1)
        {
            ProductionUnit productionUnit = productionUnits.First();
            decimal electricityProduced = productionUnit.CalculateElectricityProduced(heatProduced);
            
            expenses = heatProduced * productionUnit.ProductionCosts;
            profit = electricityProduced * sdmParameters.ElPrice;
            NetProductionCosts = expenses - profit;
        }
        
    }

    public void NetCostsForElConsumingUnitsHandler(List<ProductionUnit> productionUnits, SdmParameters sdmParameters, decimal heatConsumed)
    {
        decimal expenses;
        decimal extraExpenses;

        if (productionUnits.First().GetProductionUnitType() == -2)
        {
            ProductionUnit productionUnit = productionUnits.First();
            decimal electricityConsumed = productionUnit.CalculateElectricityConsumed(heatConsumed);

            if (productionUnit.CanReachHeatDemand(sdmParameters))
            {
                expenses = heatConsumed * productionUnit.ProductionCosts;
                extraExpenses = -electricityConsumed * sdmParameters.ElPrice;
                NetProductionCosts = expenses + extraExpenses;
            }
            else
            {
                expenses = productionUnit.MaxHeat * productionUnit.ProductionCosts;
                extraExpenses = -electricityConsumed * sdmParameters.ElPrice;
                NetProductionCosts = expenses + extraExpenses;
            }
        }
        else if(productionUnits.Last().GetProductionUnitType() == -2)
        {
            ProductionUnit productionUnit = productionUnits.Last();
            decimal electricityConsumed = productionUnit.CalculateElectricityConsumed(heatConsumed);
            
            expenses = heatConsumed * productionUnit.ProductionCosts;
            extraExpenses = -electricityConsumed * sdmParameters.ElPrice;
            NetProductionCosts = expenses + extraExpenses;
        }
    }

    public void NetCostsForHeatOnlyUnitsHandler(List<ProductionUnit> productionUnits, SdmParameters sdmParameters, decimal heatConsumed)
    {
        if (productionUnits.First().GetProductionUnitType() == -3)
        {
            ProductionUnit productionUnit = productionUnits.First();
            if (productionUnit.CanReachHeatDemand(sdmParameters))
            {
                NetProductionCosts = productionUnit.ProductionCosts * heatConsumed;
            }
            else
            {
                NetProductionCosts = productionUnit.ProductionCosts * productionUnit.MaxHeat;
            }
        }
        else if(productionUnits.Last().GetProductionUnitType() == -3)
        {
            ProductionUnit productionUnit = productionUnits.Last();
            NetProductionCosts = productionUnit.ProductionCosts * heatConsumed;
        }
    }

    public void GetBestUnitCombinations(SdmParameters sdmParameters, int primaryUnitIndex, int secondUnitIndex, decimal netWeight, decimal co2Weight)
    {
        List<ProductionUnit> unitCandidates = unitPairingCandidates;

        if (primaryUnitIndex < unitCandidates.Count)
        {
            if (secondUnitIndex < unitCandidates.Count)
            {
                if (primaryUnitIndex != secondUnitIndex)
                {
                    ProductionUnit optimalUnit = unitCandidates[primaryUnitIndex];
                    ProductionUnit optimalUnit2 = unitCandidates[secondUnitIndex];
                    List<ProductionUnit> options = new List<ProductionUnit>() {optimalUnit, optimalUnit2};

                    if (ProductionUnit.CombinedUnitsReachHeatDemand(sdmParameters, optimalUnit, optimalUnit2))
                    {
                        CalculateCo2AndNet(sdmParameters, options, netWeight, co2Weight);
                    }
                    // Recursively call the method with updated indices
                    GetBestUnitCombinations(sdmParameters, primaryUnitIndex, secondUnitIndex + 1, netWeight, co2Weight);
                }
                else
                {
                    GetBestUnitCombinations(sdmParameters, primaryUnitIndex, secondUnitIndex + 1, netWeight, co2Weight);
                }
            }
            else
            {
                // Move to the next unit when secondUnitIndex exceeds bounds
                GetBestUnitCombinations(sdmParameters, primaryUnitIndex + 1, 0, netWeight, co2Weight);
            }
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
}

public class Co2AndNetCostComparer : IComparer<Co2AndNetCost>
{
    public int Compare(Co2AndNetCost? x, Co2AndNetCost? y)
    {
        // Check if x or y is null
        if (x == null && y == null)
        {
            return 0;
        }
        else if (x == null)
        {
            return -1;
        }
        else if (y == null)
        {
            return 1;
        }

        // Compare based on productionUnits names
        if (x.ProductionUnits != null && y.ProductionUnits != null)
        {
            // Get names of productionUnits lists
            string xNames = string.Join(",", x.ProductionUnits.Select(u => u.Name));
            string yNames = string.Join(",", y.ProductionUnits.Select(u => u.Name));

            // Compare names
            return string.Compare(xNames, yNames);
        }
        else
        {
            // One or both objects have null productionUnits list
            // Compare based on null check
            return x.ProductionUnits == null ? -1 : 1;
        }
    }
}