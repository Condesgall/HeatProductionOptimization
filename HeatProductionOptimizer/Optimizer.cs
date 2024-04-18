using AssetManager_;
using ResultDataManager_;

public class Optimizer
{
    private decimal heatBoilersNetProductionCosts;
    private decimal electricityProducingNetProductionCosts;
    private decimal electricityConsumingNetProductionCosts;

    //properties
    public decimal GetElectricityConsumingNetProductionCosts
    {
        get { return electricityConsumingNetProductionCosts; }
        set { electricityConsumingNetProductionCosts = value; }
    }
    public decimal GetElectricityProducingNetProductionCosts
    {
        get { return electricityProducingNetProductionCosts;}
        set { electricityProducingNetProductionCosts = value; }
    }
    public decimal GetHeatBoilersNetProductionCosts
    {
        get { return heatBoilersNetProductionCosts;}
        set { heatBoilersNetProductionCosts = value; }
    }

    public void CalculateNetProductionCosts()
    {
        foreach (var productionUnit in AssetManager.productionUnits)
        {
            //if the production unit is elecricity producing
            if (productionUnit.MaxElectricity > 0)
            {
                electricityProducingNetProductionCosts = productionUnit.ProductionCosts - (productionUnit.MaxElectricity * productionUnit.ProductionCosts);
            }
            //if the production unit is electricity consuming
            else if (productionUnit.MaxElectricity < 0)
            {
                electricityConsumingNetProductionCosts = productionUnit.ProductionCosts + (productionUnit.MaxElectricity * productionUnit.ProductionCosts);
            }
            //if the production unit is heat only
            else
            {
                heatBoilersNetProductionCosts = productionUnit.ProductionCosts;
            }
        }
    }

    public void OptimizeProduction(List<SdmParameters> sourceData)
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
}