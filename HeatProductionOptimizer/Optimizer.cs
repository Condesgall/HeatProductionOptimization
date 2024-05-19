using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using AssetManager_;
using ResultDataManager_;

public class Optimizer
{
    public  Dictionary<ProductionUnit, decimal> individualUnitCandidates = new Dictionary<ProductionUnit, decimal>();
    public  Dictionary<ProductionUnit, decimal> unitPairingCandidates = new Dictionary<ProductionUnit, decimal>();
    public  Dictionary<List<ProductionUnit>, decimal> combinedUnitsNetCost = new Dictionary<List<ProductionUnit>, decimal>();
    public List<Co2AndNetCost> co2AndNetCostsCandidates = new List<Co2AndNetCost>();
    public List<decimal> combinedUnitsNetCostsResult = new List<decimal>();
    public List<decimal> individualUnitsNetCostsResult = new List<decimal>();
    
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

    public void OptimizeResultsSc2(List<SdmParameters> sourceData, int optimizeBy)
    {
        ResultData resultData = new ResultData();

        foreach (var sdmParameters in sourceData)
        {
            switch (optimizeBy)
            {
                //optimize by costs
                case 1:
                    OptimizeByCostsHandler(resultData, sdmParameters);
                    break;
                case 2:
                    OptimizeByCO2AndCostsHandler(sdmParameters);
                    break;
                default:
                break;
            }
        }
    }

    public void OptimizeByCostsHandler(ResultData resultData, SdmParameters sdmParameters)
    {        
        decimal bestNetCost = GetOptimizedNetCosts(sdmParameters);
        
        //checks if the most optimal net cost is from an individual unit
        if (DetectNetCostOrigin(bestNetCost) == -1)
        {
            ProductionUnit optimalUnit = individualUnitCandidates.First().Key;
            ProductionUnit optimalUnit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
            resultData.UpdateResultData(optimalUnit, optimalUnit2, bestNetCost, sdmParameters);
        }
        //or a combination of units
        else if (DetectNetCostOrigin(bestNetCost) == -2)
        {
            List<ProductionUnit> firstCombination = combinedUnitsNetCost.Keys.First();
            ProductionUnit optimalUnit = firstCombination[0];
            ProductionUnit optimalUnit2 = firstCombination[1];
            resultData.UpdateResultData(optimalUnit, optimalUnit2, bestNetCost, sdmParameters);
        }
        else
        {
            Console.WriteLine("Error.");
            return;
        }

        SaveToResultDataManager(resultData, sdmParameters);
    }

    public void OptimizeByCO2AndCostsHandler(SdmParameters sdmParameters)
    {
        ResultData resultData = new ResultData();
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        ProductionUnit unit1 = new ProductionUnit("", 0, 0, 0, 0, 0);
        List<Co2AndNetCost> optimizedResults = GetOptimizedCo2AndNet(sdmParameters, AssetManager.productionUnits);
        
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
            resultData.UpdateResultData(unit1, unit2, result.NetCost, sdmParameters);
            SaveToResultDataManager(resultData, sdmParameters);
        }
    }

    public void OptimizeByCO2EmissionHandler(SdmParameters sdmParameters)
    {
        ResultData resultData = new ResultData();
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        ProductionUnit unit1 = new ProductionUnit("", 0, 0, 0, 0, 0);
        List<Co2AndNetCost> optimizedResults = GetOptimizedCO2(sdmParameters);

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
            resultData.UpdateResultData(unit1, unit2, result.NetCost, sdmParameters);
            SaveToResultDataManager(resultData, sdmParameters);
        }
    }

    public decimal GetOptimizedNetCosts(SdmParameters sdmParameters)
    {
        GroupUnitsByDependency(sdmParameters, AssetManager.productionUnits);

        // best combinations of two units
        Dictionary<List<ProductionUnit>, decimal> bestUnitCombinations = GetBestUnitCombinations(sdmParameters, 0, 1);

        combinedUnitsNetCostsResult = bestUnitCombinations.Values.ToList();
        individualUnitsNetCostsResult = individualUnitCandidates.Values.ToList();
        
        List<decimal> sortedNetCosts = CombineAndSortNetCosts(combinedUnitsNetCostsResult, individualUnitsNetCostsResult);
        
        return sortedNetCosts[0];
    }

    public List<Co2AndNetCost> GetOptimizedCO2(SdmParameters sdmParameters)
    {
        GroupUnitsByDependency(sdmParameters, AssetManager.productionUnits);
        CalculateCo2IndividualUnits(sdmParameters);
        GetBestUnitCombinations(sdmParameters, 0, 0);
        CalculateCo2CombinedUnits(sdmParameters);
        co2AndNetCostsCandidates.OrderBy(unit => unit.Co2Emissions).ToList();

        return co2AndNetCostsCandidates;
    }
    
    public List<decimal> CombineAndSortNetCosts(List<decimal> combinedUnits, List<decimal> individualUnits)
    {
        List<decimal> allNetCosts = new List<decimal> (combinedUnits);
        allNetCosts.AddRange(individualUnits);
        allNetCosts.Sort();

        return allNetCosts;
    }

    public int DetectNetCostOrigin(decimal bestNetCost)
    {
        if (individualUnitsNetCostsResult.Contains(bestNetCost))
        {
            return -1;
        }
        else if (combinedUnitsNetCostsResult.Contains(bestNetCost))
        {
            return -2;
        }
        else
        {
            return -3;
        }
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
            decimal result = units.NetCost * units.Co2Emissions;
            units.Result = result;
            co2AndNetCostsResults.Add(units);
        }
        co2AndNetCostsResults.OrderBy(unit => unit.Result).ToList();
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
                individualUnitCandidates.Add(productionUnit, netCost);
            }
            unitPairingCandidates.Add(productionUnit, netCost);
        }
        //orders the values (ascending)
        individualUnitCandidates.OrderBy(key => key.Value).ToList();
        unitPairingCandidates.OrderBy(key => key.Value).ToList();
    }

    public void CalculateCo2IndividualUnits(SdmParameters sdmParameters)
    {
        foreach (var productionUnit in individualUnitCandidates.Keys)
        {
            decimal co2Emissions = sdmParameters.HeatDemand * productionUnit.Co2Emissions;
            decimal netCost = individualUnitCandidates[productionUnit];
            List<ProductionUnit> productionUnits = new List<ProductionUnit>() {productionUnit};

            Co2AndNetCost co2AndNetCostOfUnit = new Co2AndNetCost(productionUnits, netCost, co2Emissions, 0);
            co2AndNetCostsCandidates.Add(co2AndNetCostOfUnit);
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
        return netProductionCosts;
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
            netProductionCosts = expenses - profit;
        }
        else
        {
            expenses = productionUnit.MaxHeat * productionUnit.ProductionCosts;
            profit = productionUnit.MaxElectricity * sdmParameters.ElPrice;
            netProductionCosts = expenses - profit;
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
            netProductionCosts = expenses + extraExpenses;
        }
        else
        {
            expenses = productionUnit.MaxHeat * productionUnit.ProductionCosts;
            extraExpenses = (-productionUnit.MaxElectricity) * sdmParameters.ElPrice;
            netProductionCosts = expenses + extraExpenses;
        }
    }

    public void NetCostsForHeatOnlyUnitsHandler(ProductionUnit productionUnit, SdmParameters sdmParameters)
    {
        if (productionUnit.CanReachHeatDemand(sdmParameters))
        {
            netProductionCosts = productionUnit.ProductionCosts*sdmParameters.HeatDemand;
        }
        else
        {
            netProductionCosts = productionUnit.ProductionCosts*productionUnit.MaxHeat;
        }
    }

    /*------------------------------------------------------------------------------
        METHODS RELATED TO UNIT COMBINATIONS AND SELECTION
    --------------------------------------------------------------------------------*/

    public Dictionary<List<ProductionUnit>, decimal> GetBestUnitCombinations(SdmParameters sdmParameters, int primaryUnitIndex, int secondUnitIndex)
    {
        List<ProductionUnit> unitSortedList = unitPairingCandidates.Keys.ToList();
        List<ProductionUnit> optionsList = new List<ProductionUnit>();

        if (primaryUnitIndex == secondUnitIndex)
        {
            GetBestUnitCombinations(sdmParameters, primaryUnitIndex, secondUnitIndex + 1);
        }
        else if (primaryUnitIndex < unitSortedList.Count)
        {
            if (secondUnitIndex < unitSortedList.Count)
            {
                ProductionUnit optimalUnit = unitSortedList[primaryUnitIndex];
                ProductionUnit optimalUnit2 = unitSortedList[secondUnitIndex];

                if (ProductionUnit.CombinedUnitsReachHeatDemand(sdmParameters, optimalUnit, optimalUnit2))
                {
                    AddCombinationToDictionary(optimalUnit, optimalUnit2, optionsList);
                }
                // Recursively call the method with updated indices
                GetBestUnitCombinations(sdmParameters, primaryUnitIndex, secondUnitIndex + 1);
            }
            else
            {
                secondUnitIndex = 0;
                // Move to the next unit when secondUnitIndex exceeds bounds
                GetBestUnitCombinations(sdmParameters, primaryUnitIndex + 1, secondUnitIndex);
            }
        }
        combinedUnitsNetCost.OrderBy(pair => pair.Value).ToList();
        return combinedUnitsNetCost;
    }

    public void AddCombinationToDictionary(ProductionUnit optimalUnit, ProductionUnit unit2, List<ProductionUnit> options)
    {   
        options.Clear();
        options.Add(optimalUnit);
        options.Add(unit2);

        decimal netCost1 = unitPairingCandidates[optimalUnit];
        decimal netCost2 = unitPairingCandidates[unit2];
        
        // Calculate total net cost
        decimal totalNetCost = netCost1 + netCost2;
        
        // Add to unit options
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