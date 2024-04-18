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

    // !! OPTIMIZEBY: 1 - EXPENSES; 2 - CO2 EMISSIONS; 3 - EXPENSES&CO2 EMISSIONS
    public void OptimizeProduction(List<SdmParameters> sourceData, int optimizeBy)
    {
        foreach (var sdmParameters in sourceData)
        {
            
            ProductionUnit primaryUnit;
            ProductionUnit secondaryUnit;
            // Check whether Oil or Gas Boiler is more efficient for given parameter.
            switch(optimizeBy)
            {
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
}