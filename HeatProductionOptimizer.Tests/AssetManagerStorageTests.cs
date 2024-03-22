using System.Globalization;
using AssetManager_;
using CsvHelper;
using CsvHelper.Configuration;
using Xunit;
public class AssetManagerStorageTests
{
    AssetManagerStorage assetManagerStorage = new AssetManagerStorage();
    AssetManager assetManager = new AssetManager();
    [Fact]
    public void SaveAMData_SavesCorrectData()
    {
        //arrange
        List<ProductionUnit> loadedProductionUnitsList = new List<ProductionUnit>();
        AssetManager.heatingGridsAndProductionUnits = new Dictionary<HeatingGrid, List<ProductionUnit>>();

        //act
        assetManagerStorage.SaveAMData();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            HeaderValidated = null,
            Delimiter = ","
        };
        using (StreamReader reader = new StreamReader("heatingGrids.csv"))
        using (CsvReader csvReader = new CsvReader(reader, config))
        {
            while (csvReader.Read())
            {
                string architecture = csvReader.GetField<string>(0);
                int cityBuildings = csvReader.GetField<int>(1);
                string cityName = csvReader.GetField<string>(2);
                HeatingGrid loadedHeatingGrid = new HeatingGrid(architecture, cityBuildings, cityName);
                
                for (int i = 3; i < csvReader.ColumnCount; i++)
                {
                    var name = csvReader.GetField<string>(i);
                    var maxHeat = csvReader.GetField<double>(i + 1);
                    var productionCosts = csvReader.GetField<int>(i + 2);
                    var co2Emissions = csvReader.GetField<int>(i + 3);
                    var gasConsumption = csvReader.GetField<double>(i + 4);
                    var maxElectricity = csvReader.GetField<double>(i + 5);
                    ProductionUnit loadedProductionUnit = new ProductionUnit(name, maxHeat, productionCosts, co2Emissions, gasConsumption, maxElectricity);
                    loadedProductionUnitsList.Add(loadedProductionUnit);
                }
                AssetManager.heatingGridsAndProductionUnits.Add(loadedHeatingGrid, loadedProductionUnitsList);
            }
        }

            foreach (var heatingGrid in AssetManager.heatingGridsAndProductionUnits.Keys)
            {
                // Check if the loaded data contains the current key
                Assert.True(AssetManager.heatingGridsAndProductionUnits.ContainsKey(heatingGrid));
            }
            foreach (var listProductionUnits in AssetManager.heatingGridsAndProductionUnits.Values)
            {
                foreach (var productionUnit in listProductionUnits)
                {
                    Assert.Contains(productionUnit, listProductionUnits);
                }
            }
        File.Delete("heatingGrid.csv");
    }
    [Fact]
    public void LoadsAMData_LoadsCorrectData()
    {
        File.Create("heatingGrids.csv").Close();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            HeaderValidated = null,
            Delimiter = ","
        };
        using (var writer = new StreamWriter("heatingGrids.csv"))
        using (var csv = new CsvWriter(writer, config))
        {
            foreach (var heatingGridAndProductionUnits in AssetManager.heatingGridsAndProductionUnits.Keys)
            {
                csv.WriteField(AssetManager.heatingGrid.Architecture);
                csv.WriteField(AssetManager.heatingGrid.CityBuildings);
                csv.WriteField(AssetManager.heatingGrid.CityName);
            }
                
            foreach (var listProductionUnits in AssetManager.heatingGridsAndProductionUnits.Values)
            {
                foreach (var productionUnit in listProductionUnits)
                {
                    csv.WriteField(productionUnit.Name);
                    csv.WriteField(productionUnit.MaxHeat);
                    csv.WriteField(productionUnit.ProductionCosts);
                    csv.WriteField(productionUnit.Co2Emissions);
                    csv.WriteField(productionUnit.GasConsumption);
                    csv.WriteField(productionUnit.MaxElectricity);
                }
            }
        }

        assetManagerStorage.LoadAMData();

        foreach (var heatingGrid in AssetManager.heatingGridsAndProductionUnits.Keys)
        {
            // Check if the loaded data contains the current key
            Assert.True(AssetManager.heatingGridsAndProductionUnits.ContainsKey(heatingGrid));
        }
        foreach (var listProductionUnits in AssetManager.heatingGridsAndProductionUnits.Values)
        {
            foreach (var productionUnit in listProductionUnits)
            {
                Assert.Contains(productionUnit, listProductionUnits);
            }
        }   
        File.Delete("heatingGrids.csv");
    }
}