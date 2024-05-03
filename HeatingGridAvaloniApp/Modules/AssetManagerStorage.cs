using System.Globalization;
using HeatingGridAvaloniaApp.Modules;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System;
using System.Collections.Generic;

namespace HeatingGridAvaloniaApp.Modules;

public interface IAssetManagerStorage
{
    abstract void LoadAMData();
    abstract void SaveAMData();
}

public class AssetManagerStorage : IAssetManagerStorage
{
    public void LoadAMData()
    {
        try
        {
            if (File.Exists("../Assets/heatingGrids.csv"))
            {
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
                            var maxHeat = csvReader.GetField<decimal>(i + 1);
                            var productionCosts = csvReader.GetField<int>(i + 2);
                            var co2Emissions = csvReader.GetField<int>(i + 3);
                            var gasConsumption = csvReader.GetField<decimal>(i + 4);
                            var maxElectricity = csvReader.GetField<decimal>(i + 5);
                            ProductionUnit loadedProductionUnit = new ProductionUnit(name, maxHeat, productionCosts, co2Emissions, gasConsumption, maxElectricity);
                            if (!AssetManager.productionUnits.Contains(loadedProductionUnit))
                            {
                                AssetManager.productionUnits.Add(loadedProductionUnit);
                            }
                        }
                        if (!AssetManager.heatingGridsAndProductionUnits.ContainsKey(loadedHeatingGrid) && !AssetManager.heatingGridsAndProductionUnits.ContainsValue(AssetManager.productionUnits))
                        {
                            AssetManager.heatingGridsAndProductionUnits.Add(loadedHeatingGrid, AssetManager.productionUnits);
                        }
                    }
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found. {ex}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error. {ex}");
            throw;
        }
    }

    public void SaveAMData()
    {
        //Checks if file exists
        if (!File.Exists("heatingGrids.csv"))
        {
            File.Create("heatingGrids.csv").Close();
        }
        //configuration for the csv reader
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            //indicates that CSV has no header
            HasHeaderRecord = false,
            //will throw a ValidationException if there is no header
            HeaderValidated = null,
            //separates fields with comm
            Delimiter = ","
        };
        using (var writer = new StreamWriter("heatingGrids.csv"))
        using (var csv = new CsvWriter(writer, config))
        {
            //writes all the heating grid properties for each heating grid in the dictionary
            foreach (var heatingGridAndProductionUnits in AssetManager.heatingGridsAndProductionUnits.Keys)
            {
                csv.WriteField(AssetManager.heatingGrid.Architecture);
                csv.WriteField(AssetManager.heatingGrid.CityBuildings);
                csv.WriteField(AssetManager.heatingGrid.CityName);
            }
            //writes the list of all the production unit associated with the heating grid
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
    }
}