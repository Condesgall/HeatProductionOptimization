using AssetManager_;

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

     public void OptimizeProduction(ParameterLoader parameterLoader, AssetManager_.AssetManager assetManager, ResultDataManager_.ResultDataManager resultDataManager)
    {
        double producedHeat = 0;
        double producedElectricity = 0;
        double consumedElectricity = 0;
        double expenses = 0;
        double profit = 0;
        double primaryEnergyConsumption = 0;
        double co2Emissions = 0;

        // Loop  winter data
        foreach (var winterData in parameterLoader.Winter)
        {
            foreach (var productionUnit in AssetManager_.AssetManager.productionUnits)
            {
                //Only for scenario 1
                if (productionUnit.Name == "GB" || productionUnit.Name == "OB" ){
                    
                    
                    
                    
                    var optimizationResults = new ResultDataManager_.OptimizationResults(
                        producedHeat,
                        producedElectricity,
                        consumedElectricity,
                        expenses,
                        profit,
                        primaryEnergyConsumption,
                        co2Emissions
                    );

                    // Add optimization results
                    resultDataManager.AddResultData(productionUnit.Name, optimizationResults);
                }
                else
                {
                    continue;
                }
            }
        }

        // Loop summer data
        foreach (var summerData in parameterLoader.Summer)
        {
            foreach (var productionUnit in AssetManager_.AssetManager.productionUnits)
            {
               //Only for scenario 1
                if (productionUnit.Name == "GB" || productionUnit.Name == "OB" ){
                    
                    
                    
                    
                    var optimizationResults = new ResultDataManager_.OptimizationResults(
                        producedHeat,
                        producedElectricity,
                        consumedElectricity,
                        expenses,
                        profit,
                        primaryEnergyConsumption,
                        co2Emissions
                    );

                    // Add optimization results
                    resultDataManager.AddResultData(productionUnit.Name, optimizationResults);
                }
                else
                {
                    continue;
                }
            }
        }
    }
}