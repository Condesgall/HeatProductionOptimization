namespace HeatProductionOptimizer.Tests;

public class AssetManagerTests
{
    [Fact]
    public void ProductionUnit_WithValidParameters_SetsPropertiesCorrectly()
    {
        //ARRANGE
        string name = "GB";
        double maxHeat = 5.0;
        int productionCosts = 500;
        int co2Emissions = 215;
        double gasConsumption = 1.1;
        double maxElectricity = 2.7;

        //ACT
        ProductionUnit gasBoiler = new ProductionUnit(name, maxHeat, productionCosts, co2Emissions, gasConsumption, maxElectricity);

        //ASSERT
        Assert.Equal(name, gasBoiler.GetName());
        Assert.Equal(maxHeat, gasBoiler.GetMaxHeat());
        Assert.Equal(productionCosts, gasBoiler.GetProductionCosts());
        Assert.Equal(co2Emissions, gasBoiler.GetCO2Emissions());
        Assert.Equal(gasConsumption, gasBoiler.GetGasConsumption());
        Assert.Equal(maxElectricity, gasBoiler.GetMaxElectricity());
    }

    [Fact]
    public void HeatingGrid_WithValidparameters_SetsPropertiesCorrectly()
    {
        //ARRANGE
        string architecture = "Singe District Heating Network";
        int cityBuildings = 1600;
        string cityName = "Heatington";

        //ACT
        HeatingGrid city = new HeatingGrid(architecture, cityBuildings, cityName);   

        //ASSERT
        Assert.Equal(architecture, city.GetArchitecture());
        Assert.Equal(cityBuildings, city.GetCityBuildings());
        Assert.Equal(cityName, city.GetCityName());
    }
}