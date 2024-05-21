using HeatingGridAvaloniaApp.Models;
namespace HeatProductionOptimizer.Tests;
public class AssetManagerTests
{
    [Fact]
    public void ProductionUnit_WithValidParameters_SetsPropertiesCorrectly()
    {
        //ARRANGE
        string name = "GB";
        decimal maxHeat = 5.0m;
        int productionCosts = 500;
        int co2Emissions = 215;
        decimal gasConsumption = 1.1m;
        decimal maxElectricity = 2.7m;

        //ACT
        ProductionUnit gasBoiler = new ProductionUnit(name, maxHeat, productionCosts, co2Emissions, gasConsumption, maxElectricity);

        //ASSERT
        Assert.Equal(name, gasBoiler.Name);
        Assert.Equal(maxHeat, gasBoiler.MaxHeat);
        Assert.Equal(productionCosts, gasBoiler.ProductionCosts);
        Assert.Equal(co2Emissions, gasBoiler.Co2Emissions);
        Assert.Equal(gasConsumption, gasBoiler.GasConsumption);
        Assert.Equal(maxElectricity, gasBoiler.MaxElectricity);
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
        Assert.Equal(architecture, city.Architecture);
        Assert.Equal(cityBuildings, city.CityBuildings);
        Assert.Equal(cityName, city.CityName);
    }
}