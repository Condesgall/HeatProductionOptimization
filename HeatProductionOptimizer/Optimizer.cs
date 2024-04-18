using AssetManager_;
using ResultDataManager_;

public class Optimizer
{
    public static Dictionary<ProductionUnit, decimal> productionUnitNetCosts = new Dictionary<ProductionUnit, decimal>();
    private decimal heatBoilersNetProductionCosts;
    private decimal elProducingNetProductionCosts;
    private decimal elConsumingNetProductionCosts;

    //properties
    public decimal GetElConsumingNetProductionCosts
    {
        get { return elConsumingNetProductionCosts; }
        set { elConsumingNetProductionCosts = value; }
    }
    public decimal GetElProducingNetProductionCosts
    {
        get { return elProducingNetProductionCosts;}
        set { elProducingNetProductionCosts = value; }
    }
    public decimal GetHeatBoilersNetProductionCosts
    {
        get { return heatBoilersNetProductionCosts;}
        set { heatBoilersNetProductionCosts = value; }
    }

    public decimal CalculateNetProductionCosts(SdmParameters sdmParameters)
    {
        foreach (var productionUnit in AssetManager.productionUnits)
        {
            switch (productionUnit.MaxElectricity)
            {
                //production unit produces electricity
                case >0:
                //if electricity that can be produced isnt over the limit
                    if (sdmParameters.HeatDemand <= productionUnit.MaxElectricity)
                    {
                        elProducingNetProductionCosts = sdmParameters.HeatDemand * (productionUnit.ProductionCosts - sdmParameters.ElPrice);
                    }
                    //if it is over the limit
                    else if (sdmParameters.HeatDemand > productionUnit.MaxElectricity)
                    {
                        elProducingNetProductionCosts = sdmParameters.HeatDemand * productionUnit.ProductionCosts - productionUnit.MaxElectricity * sdmParameters.ElPrice;
                    }
                    //if the heat demand is bigger than the max heat
                    else
                    {
                        elProducingNetProductionCosts = productionUnit.MaxHeat * productionUnit.ProductionCosts - productionUnit.MaxElectricity * sdmParameters.ElPrice;
                    }
                    return elConsumingNetProductionCosts;
                //production unit consumes electricity
                case <0:
                    //if heat demand doesnt surpass the heat limit
                    if (sdmParameters.HeatDemand <= productionUnit.MaxHeat)
                    {
                        elConsumingNetProductionCosts = sdmParameters.HeatDemand * (productionUnit.ProductionCosts + sdmParameters.ElPrice);
                    }
                    else
                    {
                        elConsumingNetProductionCosts = productionUnit.MaxHeat * (productionUnit.ProductionCosts + sdmParameters.ElPrice);
                    }
                    return elConsumingNetProductionCosts;
                //heat only
                case 0:
                    //if heat demand doesnt surpass the heat limit
                    if (sdmParameters.HeatDemand <= productionUnit.MaxHeat)
                    {
                        heatBoilersNetProductionCosts = productionUnit.ProductionCosts*sdmParameters.HeatDemand;
                    }
                    else
                    {
                        heatBoilersNetProductionCosts = productionUnit.ProductionCosts*productionUnit.MaxHeat;
                    }
                    return heatBoilersNetProductionCosts;
                default:
            }
        }
        return 0;
    }
    public void GetProductionUnitsNetCosts(SdmParameters sdmParameters)
    {
        foreach (var productionUnit in AssetManager.productionUnits)
        {
            decimal netCost = CalculateNetProductionCosts(sdmParameters);
            productionUnitNetCosts.Add(productionUnit, netCost);
        }
        productionUnitNetCosts.OrderBy(key => key.Value);
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
    
    // SCENARIO 2

    //not complete
    public void OptimizeProduction(List<SdmParameters> sourceData)
    {
        foreach (var sdmParameters in sourceData)
        {
            GetProductionUnitsNetCosts(sdmParameters);
        }
    }
}