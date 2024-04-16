using AssetManager_;
using ResultDataManager_;

public class Optimizer
{
    private double heatBoilersNetProductionCosts;
    private double electricityProducingNetProductionCosts;
    private double electricityConsumingNetProductionCosts;

    //properties
    public double GetElectricityConsumingNetProductionCosts
    {
        get { return electricityConsumingNetProductionCosts; }
        set { electricityConsumingNetProductionCosts = value; }
    }
    public double GetElectricityProducingNetProductionCosts
    {
        get { return electricityProducingNetProductionCosts;}
        set { electricityProducingNetProductionCosts = value; }
    }
    public double GetHeatBoilersNetProductionCosts
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
            double currentHeatNeeded = SdmParameters.HeatDemand;
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

                ResultDataManager.Summer.Add(resultData);
            }
            else
            {
                optimizationResults.ProducedHeat = AssetManager.productionUnits[0].MaxHeat;
                optimizationResults.Expenses = AssetManager.productionUnits[0].MaxHeat * AssetManager.productionUnits[0].ProductionCosts;
                optimizationResults.PrimaryEnergyConsumption = AssetManager.productionUnits[0].MaxHeat * AssetManager.productionUnits[0].GasConsumption;
                optimizationResults.Co2Emissions = AssetManager.productionUnits[0].MaxHeat * AssetManager.productionUnits[0].Co2Emissions;

                resultData.TimeFrom = SdmParameters.TimeFrom;
                resultData.TimeTo = SdmParameters.TimeTo;
                resultData.ProductionUnit = "GB";
                resultData.OptimizationResults = optimizationResults;

                ResultDataManager.Winter.Add(resultData);

                currentHeatNeeded -= AssetManager.productionUnits[0].MaxHeat;
                OptimizationResults optimizationResults2 = new OptimizationResults(0,0,0,0,0,0,0);
                ResultData resultData2 = new ResultData("","","",optimizationResults);

                optimizationResults2.ProducedHeat = currentHeatNeeded;
                optimizationResults2.Expenses = currentHeatNeeded * AssetManager.productionUnits[0].ProductionCosts;
                optimizationResults2.PrimaryEnergyConsumption = currentHeatNeeded * AssetManager.productionUnits[0].GasConsumption;
                optimizationResults2.Co2Emissions = currentHeatNeeded * AssetManager.productionUnits[0].Co2Emissions;

                resultData2.TimeFrom = SdmParameters.TimeFrom;
                resultData2.TimeTo = SdmParameters.TimeTo;
                resultData2.ProductionUnit = "OB";
                resultData2.OptimizationResults = optimizationResults2;

                ResultDataManager.Winter.Add(resultData2);
            }
        

            // Add optimization results
        }
    }
}