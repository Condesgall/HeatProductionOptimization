using AssetManager_;
using ResultDataManager_;

public class Optimizer
{
    public  Dictionary<ProductionUnit, decimal> bestIndividualUnits = new Dictionary<ProductionUnit, decimal>();
    public  Dictionary<ProductionUnit, decimal> unitPairingCandidatesNetCosts = new Dictionary<ProductionUnit, decimal>();
    public  Dictionary<List<ProductionUnit>, decimal> combinedUnitsNetCost = new Dictionary<List<ProductionUnit>, decimal>();
    public List<decimal> combinedUnitsNetCostsList = new List<decimal>();
    public List<decimal> individualUnitsNetCostsList = new List<decimal>();
    
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
                    //OptimizeByCO2AndCostsHandler();
                    break;
                default:
                break;
            }
        }
    }

    public void OptimizeByCostsHandler(ResultData resultData, SdmParameters sdmParameters)
    {        
        decimal bestNetCost = GetBestNetCost(sdmParameters);

        //checks if the most optimal net cost is from an individual unit
        if (DetectNetCostOrigin(bestNetCost) == -1)
        {
            ProductionUnit optimalUnit = bestIndividualUnits.First().Key;
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


    /*-------------------------------------------------------------------------------
        METHODS RELATED TO NET COST CALCULATIONS
    ---------------------------------------------------------------------------------*/

    public decimal GetBestNetCost(SdmParameters sdmParameters)
    {
        // best combinations of two units
        Dictionary<List<ProductionUnit>, decimal> bestUnitCombinations = GetBestUnitCombinations(sdmParameters, 0, 1);

        combinedUnitsNetCostsList = bestUnitCombinations.Values.ToList();
        individualUnitsNetCostsList = bestIndividualUnits.Values.ToList();
        
        List<decimal> sortedNetCosts = CombineAndSortNetCosts(combinedUnitsNetCostsList, individualUnitsNetCostsList);
        
        return sortedNetCosts[0];
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
        if (individualUnitsNetCostsList.Contains(bestNetCost))
        {
            return -1;
        }
        else if (combinedUnitsNetCostsList.Contains(bestNetCost))
        {
            return -2;
        }
        else
        {
            return -3;
        }
    }


    /*--------------------------------------------------------------------------------
        METHODS RELATED TO INDIVIDUAL UNIT CALCULATIONS
    ---------------------------------------------------------------------------------*/

    public List<ProductionUnit> GetBestIndividualUnits(SdmParameters sdmParameters)
    {
        GroupUnitsByDependency(sdmParameters, AssetManager.productionUnits);
        List<ProductionUnit> individualUnitSortedList = new List<ProductionUnit> (bestIndividualUnits.Keys);
        return individualUnitSortedList;
    }

    public void GroupUnitsByDependency(SdmParameters sdmParameters, List<ProductionUnit> productionUnits)
    {
        foreach (var productionUnit in productionUnits)
        {
            decimal netCost = CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);

            if (productionUnit.CanReachHeatDemand(sdmParameters))
            {
                bestIndividualUnits.Add(productionUnit, netCost);
            }
            unitPairingCandidatesNetCosts.Add(productionUnit, netCost);
        }
        //orders the values (ascending)
        bestIndividualUnits.OrderBy(key => key.Value);
        unitPairingCandidatesNetCosts.OrderBy(key => key.Value);
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
        List<ProductionUnit> unitSortedList = unitPairingCandidatesNetCosts.Keys.ToList();
        List<ProductionUnit> optionsList = new List<ProductionUnit>();

        if (primaryUnitIndex < unitSortedList.Count)
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
                // Move to the next unit when secondUnitIndex exceeds bounds
                GetBestUnitCombinations(sdmParameters, primaryUnitIndex + 1, secondUnitIndex + 2);
            }
        }
        combinedUnitsNetCost.OrderBy(pair => pair.Value);
        return combinedUnitsNetCost;
    }

    public void AddCombinationToDictionary(ProductionUnit optimalUnit, ProductionUnit unit2, List<ProductionUnit> options)
    {   
        options.Clear();
        options.Add(optimalUnit);
        options.Add(unit2);

        decimal netCost1 = unitPairingCandidatesNetCosts[optimalUnit];
        decimal netCost2 = unitPairingCandidatesNetCosts[unit2];
        
        // Calculate total net cost
        decimal totalNetCost = netCost1 + netCost2;
        
        // Add to unit options
        if (!combinedUnitsNetCost.ContainsKey(options))
        {
            combinedUnitsNetCost.Add(options, totalNetCost);
        }
    }
}