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
}