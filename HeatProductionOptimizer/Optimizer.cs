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

    public void OptimizeProductionByCosts(List<SdmParameters> sourceData)
    {
        foreach (var SdmParameters in sourceData)
        {
            decimal currentHeatNeeded = SdmParameters.HeatDemand;
            OptimizationResults optimizationResults = new OptimizationResults(0,0,0,0,0,0,0);
            ResultData resultData = new ResultData("","","",optimizationResults);

            // Check if Gas Boiler itself can meet the heat demand
            if(currentHeatNeeded <= AssetManager.productionUnits[0].MaxHeat)
            {
                optimizationResults.ProducedHeat = currentHeatNeeded;
                optimizationResults.Expenses = currentHeatNeeded * AssetManager.productionUnits[0].ProductionCosts;
                optimizationResults.PrimaryEnergyConsumption = currentHeatNeeded * AssetManager.productionUnits[0].GasConsumption;
                optimizationResults.Co2Emissions = currentHeatNeeded * AssetManager.productionUnits[0].Co2Emissions;

                resultData.TimeFrom = SdmParameters.TimeFrom;
                resultData.TimeTo = SdmParameters.TimeTo;
                resultData.ProductionUnit = "GB";
                resultData.OptimizationResults = optimizationResults;

                //adds results to list (summer)
                ResultDataManager.Summer.Add(resultData);
            }
            else
            {
                //if Gas Boiler itself can't meet the heat demand, it uses max heat, so produced heat for this unit is the max heat
                optimizationResults.ProducedHeat = AssetManager.productionUnits[0].MaxHeat;
                //calculation of expenses
                optimizationResults.Expenses = AssetManager.productionUnits[0].MaxHeat * AssetManager.productionUnits[0].ProductionCosts;
                //calculates consumption of gas
                optimizationResults.PrimaryEnergyConsumption = AssetManager.productionUnits[0].MaxHeat * AssetManager.productionUnits[0].GasConsumption;
                //calculates co2 emissions
                optimizationResults.Co2Emissions = AssetManager.productionUnits[0].MaxHeat * AssetManager.productionUnits[0].Co2Emissions;

                resultData.TimeFrom = SdmParameters.TimeFrom;
                resultData.TimeTo = SdmParameters.TimeTo;
                resultData.ProductionUnit = "GB";
                resultData.OptimizationResults = optimizationResults;

                //adds result data to list (winter)
                ResultDataManager.Winter.Add(resultData);

                //subtracts the maximum heat from oil boiler from the current heat needed
                currentHeatNeeded -= AssetManager.productionUnits[1].MaxHeat;
                OptimizationResults optimizationResults2 = new OptimizationResults(0,0,0,0,0,0,0);
                ResultData resultData2 = new ResultData("","","",optimizationResults);

                optimizationResults2.ProducedHeat = currentHeatNeeded;
                optimizationResults2.Expenses = currentHeatNeeded * AssetManager.productionUnits[1].ProductionCosts;
                optimizationResults2.PrimaryEnergyConsumption = currentHeatNeeded * AssetManager.productionUnits[1].GasConsumption;
                optimizationResults2.Co2Emissions = currentHeatNeeded * AssetManager.productionUnits[1].Co2Emissions;

                resultData2.TimeFrom = SdmParameters.TimeFrom;
                resultData2.TimeTo = SdmParameters.TimeTo;
                resultData2.ProductionUnit = "OB";
                resultData2.OptimizationResults = optimizationResults2;

                ResultDataManager.Winter.Add(resultData2);
            }
        }
    }

    // SCENARIO 2
    // 

    //not complete
    public void OptimizeProduction(List<SdmParameters> sourceData)
    {
        foreach (var sdmParameters in sourceData)
        {
            GetProductionUnitsNetCosts(sdmParameters);
        }
    }
}