using Newtonsoft.Json;
using Xunit;

public class AssetManagerJsonTests
{
    AssetManagerJson assetManagerJson = new AssetManagerJson();
    [Fact]
    public void SaveAMData_SavesCorrectData()
    {
        //arrange
        ProductionUnit productionUnit = new ProductionUnit("GB", 5.0, 500, 215, 1.1, 0);
        ProductionUnit oilBoiler = new ProductionUnit("OB", 4.0, 700, 265, 1.2, 0);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit, oilBoiler };
        HeatingGrid Heatington = new HeatingGrid("Single District Heating Network", 1600, "Heatington", productionUnits);
        File.Create("heatingGrid.json").Close();

        //act
        assetManagerJson.SaveAMData(Heatington);
        string dataInJson = File.ReadAllText("heatingGrid.json");
        HeatingGrid? deserializedHeatington = JsonConvert.DeserializeObject<HeatingGrid>(dataInJson);

        //assert
        if (deserializedHeatington != null)
        {
            Assert.Equal(Heatington.GetArchitecture, deserializedHeatington.GetArchitecture);
            Assert.Equal(Heatington.GetCityBuildings, deserializedHeatington.GetCityBuildings);
            Assert.Equal(Heatington.GetCityName, deserializedHeatington.GetCityName);

            List<ProductionUnit> productionUnitsList = Heatington.GetProductionUnits.ToList();
            List<ProductionUnit> deserializedProductionUnitsList = deserializedHeatington.GetProductionUnits.ToList();

            for (int i = 0; i < Heatington.GetProductionUnits.Count(); i++)
            {
                Assert.Equal(productionUnitsList[i].GetName, deserializedProductionUnitsList[i].GetName);
                Assert.Equal(productionUnitsList[i].GetMaxHeat, deserializedProductionUnitsList[i].GetMaxHeat);
                Assert.Equal(productionUnitsList[i].GetProductionCosts, deserializedProductionUnitsList[i].GetProductionCosts);
                Assert.Equal(productionUnitsList[i].GetCO2Emissions, deserializedProductionUnitsList[i].GetCO2Emissions);
                Assert.Equal(productionUnitsList[i].GetGasConsumption, deserializedProductionUnitsList[i].GetGasConsumption);
                Assert.Equal(productionUnitsList[i].GetMaxElectricity, deserializedProductionUnitsList[i].GetMaxElectricity);
            }
        }
        File.Delete("heatingGrid.json");
    }

    [Fact]
    public void LoadAMData_LoadsCorrectData()
    {
        ProductionUnit productionUnit = new ProductionUnit("GB", 5.0, 500, 215, 1.1, 0);
        ProductionUnit oilBoiler = new ProductionUnit("OB", 4.0, 700, 265, 1.2, 0);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit, oilBoiler };
        HeatingGrid Heatington = new HeatingGrid("Single District Heating Network", 1600, "Heatington", productionUnits);
        File.Create("heatingGrid.json").Close();

        //act
        assetManagerJson.SaveAMData(Heatington);
        HeatingGrid deserializedHeatington = assetManagerJson.LoadAMData();

        if (deserializedHeatington != null)
        {
            Assert.Equal(Heatington.GetArchitecture, deserializedHeatington.GetArchitecture);
            Assert.Equal(Heatington.GetCityBuildings, deserializedHeatington.GetCityBuildings);
            Assert.Equal(Heatington.GetCityName, deserializedHeatington.GetCityName);

            List<ProductionUnit> productionUnitsList = Heatington.GetProductionUnits.ToList();
            List<ProductionUnit> deserializedProductionUnitsList = deserializedHeatington.GetProductionUnits.ToList();

            for (int i = 0; i < Heatington.GetProductionUnits.Count(); i++)
            {
                Assert.Equal(productionUnitsList[i].GetName, deserializedProductionUnitsList[i].GetName);
                Assert.Equal(productionUnitsList[i].GetMaxHeat, deserializedProductionUnitsList[i].GetMaxHeat);
                Assert.Equal(productionUnitsList[i].GetProductionCosts, deserializedProductionUnitsList[i].GetProductionCosts);
                Assert.Equal(productionUnitsList[i].GetCO2Emissions, deserializedProductionUnitsList[i].GetCO2Emissions);
                Assert.Equal(productionUnitsList[i].GetGasConsumption, deserializedProductionUnitsList[i].GetGasConsumption);
                Assert.Equal(productionUnitsList[i].GetMaxElectricity, deserializedProductionUnitsList[i].GetMaxElectricity);
            }
        }
        File.Delete("heatingGrid.json");
    }
}