using System;
using System.Collections.Generic;
using System.Globalization;
using HeatingGridAvaloniaApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Collections.Generic;
using Avalonia;

namespace HeatingGridAvaloniaApp.Models

{
    public class AssetManager
    {
        //list of production units for the project
        public static List<ProductionUnit> productionUnits = new List<ProductionUnit>
        {
            new ProductionUnit("GB", 5.0m, 500, 215, 1.1m, 0), //heat only boiler
            new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0), //heat only boiler
            new ProductionUnit("GM", 3.6m, 1100, 640, 1.9m, 2.7m), //electricity producing unit
            new ProductionUnit("EK", 8.0m, 50, 0, 0, -8.0m) //electricity consuming units
        };

        //heating grid for the project
        public static HeatingGrid heatingGrid = new HeatingGrid("single district heating network", 1600, "Heatington");

        //dictionary that stores heating grids and respective production units
        public static Dictionary<HeatingGrid,List<ProductionUnit>> heatingGridsAndProductionUnits = new Dictionary<HeatingGrid,List<ProductionUnit>>()
        {
            {heatingGrid, productionUnits}
        };

        public void DisplayAssetManager()
        {
            Console.WriteLine("_____________________________");
            Console.WriteLine("Heating area: ");
            Console.WriteLine("_____________________________");
            Console.WriteLine("");
            Console.WriteLine($"City name: {heatingGrid.CityName}");
            Console.WriteLine($"Number of buildings: {heatingGrid.CityBuildings}");
            Console.WriteLine($"Architecture: {heatingGrid.Architecture}");
            Console.WriteLine("");
            Console.WriteLine("_____________________________");
            Console.WriteLine("Production units: ");
            Console.WriteLine("_____________________________");
            foreach (var productionUnit in productionUnits)
            {
                Console.WriteLine($"Name: {productionUnit.Name}");
                Console.WriteLine($"Maximum heat: {productionUnit.MaxHeat}");
                Console.WriteLine($"Maximum electricity: {productionUnit.MaxElectricity}");
                Console.WriteLine($"Production costs: {productionUnit.ProductionCosts}");
                Console.WriteLine($"CO2 emissions: {productionUnit.Co2Emissions}");
                Console.WriteLine($"Gas consumption: {productionUnit.GasConsumption}");
                Console.WriteLine("_____________________________");
            }
        }
    }

    public class ProductionUnit
    {
        private string name;
        private decimal maxHeat;
        private int productionCosts;
        private int co2Emissions;
        private decimal gasConsumption;
        private decimal maxElectricity;

        //todo add image
        //constructor for the production unit class
        public ProductionUnit(string name_, decimal maxHeat_, int productionCosts_, int co2Emissions_, decimal gasConsumption_, decimal maxElectricity_)
        {
            name = name_;
            maxHeat = maxHeat_;
            productionCosts = productionCosts_;
            co2Emissions = co2Emissions_;
            gasConsumption = gasConsumption_;
            maxElectricity = maxElectricity_;
        }

        //properties for the variables
        public string Name
        {
            get {return name; }
            set {name = value;}
        }

        public decimal MaxHeat
        {
            get {return maxHeat; }
            set {maxHeat = value;}
        }

        public int ProductionCosts
        {
            get {return productionCosts; }
            set {productionCosts = value;}
        }

        public int Co2Emissions
        {
            get {return co2Emissions; }
            set {co2Emissions = value;}
        }

        public decimal GasConsumption
        {
            get {return gasConsumption; }
            set {gasConsumption = value;}
        }

        public decimal MaxElectricity
        {
            get {return maxElectricity; }
            set {maxElectricity = value;}
        }

         //Sets all the properties to 0
        public void SwitchOffProductionUnit(){
            MaxHeat = 0;
            ProductionCosts = 0;
            Co2Emissions = 0;
            GasConsumption = 0;
            MaxElectricity = 0;
        }
    }

    public class HeatingGrid
    {
        private string architecture;
        private int cityBuildings;
        private string cityName;

        //constructor for the heating grid class
        public HeatingGrid(string architecture_, int cityBuildings_, string cityName_)
        {
            architecture = architecture_;
            cityBuildings = cityBuildings_;
            cityName = cityName_;
        }

        //properties
        public string Architecture
        {
            get {return architecture; }
            set {architecture = value;}
        }

        public int CityBuildings
        {
            get {return cityBuildings; }
            set {cityBuildings = value;}
        }

        public string CityName
        {
            get {return cityName; }
            set {cityName = value;}
        }
    }
}
