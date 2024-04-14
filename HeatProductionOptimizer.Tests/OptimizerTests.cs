using AssetManager_;
public class AssetManagerTests
{
    public Optimizer optimizer = new Optimizer();

    [Fact]
    public void CalculateNetProductionCosts_WhenElectricityProducing_UpdatesElectricityProducingNetProductionCosts()
    {
        //arranges
        ProductionUnit productionUnit = new ProductionUnit("GM", 3.6, 1100, 640, 1.9, 2.7);
        AssetManager.productionUnits = [productionUnit];
        //act
        optimizer.CalculateNetProductionCosts();
        //assert
        Assert.True(optimizer.GetElectricityProducingNetProductionCosts == -1870);
    }

    [Fact]
    public void CalculateNetProductionCosts_WhenElectricityConsuming_UpdatesElectricityProducingNetProductionCosts()
    {
        //arranges
        ProductionUnit productionUnit = new ProductionUnit("EK", 8.0, 50, 0, 0, -8.0);
        AssetManager.productionUnits = [productionUnit];
        //act
        optimizer.CalculateNetProductionCosts();
        //assert
        Assert.True(optimizer.GetElectricityConsumingNetProductionCosts == -350);
    }

    [Fact]
    public void CalculateNetProductionCosts_WhenHeatOnly_UpdatesElectricityProducingNetProductionCosts()
    {
        //arranges
        ProductionUnit productionUnit = new ProductionUnit("OB", 4.0, 700, 265, 1.2, 0);
        AssetManager.productionUnits = [productionUnit];
        //act
        optimizer.CalculateNetProductionCosts();
        //assert
        Assert.True(optimizer.GetHeatBoilersNetProductionCosts == 700);
    }
}