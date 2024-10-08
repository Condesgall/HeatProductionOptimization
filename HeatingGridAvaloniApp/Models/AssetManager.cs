using System;
using System.Collections.Generic;
using System.Globalization;
using HeatingGridAvaloniaApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using Avalonia;
using System.Reflection;

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
    }

    public class ProductionUnit
    {
        private string name;
        private decimal maxHeat;
        private decimal productionCosts;
        private decimal co2Emissions;
        private decimal gasConsumption;
        private decimal maxElectricity;

        //todo add image
        //constructor for the production unit class
        public ProductionUnit(string name_, decimal maxHeat_, decimal productionCosts_, decimal co2Emissions_, decimal gasConsumption_, decimal maxElectricity_)
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

        public decimal ProductionCosts
        {
            get {return productionCosts; }
            set {productionCosts = value;}
        }

        public decimal Co2Emissions
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

        /// <summary>
        /// Checks if the unit is electricity producing, consuming, or neither.
        /// </summary>
        /// <remarks>
        /// Usage Codes:
        /// -1: Electricity producing
        /// -2: Electricity consuming.
        /// -3: Doesn't use electricity.
        /// </remarks>
        public int GetProductionUnitType()
        {
            if (MaxElectricity > 0)
            {
                return -1;
            }
            else if (MaxElectricity < 0)
            {
                return -2;
            }
            else if (MaxElectricity == 0)
            {
                return -3;
            }
            else 
            {
                bool allZero = true; // Flag to track if all properties are zero
                PropertyInfo[] properties = typeof(ProductionUnit).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    object? value = property.GetValue(this);
                    if (value != null && (int)value != 0)
                    {
                        allZero = false; 
                        break; 
                    }
                }
                if (allZero)
                {
                    return -4; // Return -4 if all properties are zero
                }
            }
            return -5;
        }

        public decimal CalculateElectricityProduced(decimal heatDemand)
        {
            decimal electricityProduced = (heatDemand/MaxHeat) * MaxElectricity;
            if (electricityProduced <= MaxElectricity)
            {
                return electricityProduced;
            }
            else
            {
                return MaxElectricity;
            }
        }

        public decimal CalculateElectricityConsumed(decimal heatDemand)
        {
                decimal electricityConsumed = (heatDemand / MaxHeat) * MaxElectricity;
            
                if (electricityConsumed > MaxElectricity)
                {
                    return MaxElectricity;
                }
                else
                {
                    return electricityConsumed;
                }
        }


        public bool CanReachHeatDemand(SdmParameters sdmParameters)
        {
            if (sdmParameters.HeatDemand > MaxHeat)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool CombinedUnitsReachHeatDemand(SdmParameters sdmParameters, ProductionUnit unit1, ProductionUnit unit2)
        {
            if (sdmParameters.HeatDemand > unit1.MaxHeat && sdmParameters.HeatDemand < unit1.MaxHeat + unit2.MaxHeat)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsElectricBoiler()
        {
            if (GasConsumption == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IfThereIsASecondUnit()
        {
            if (Name == "")
            {
                return false;
            }
            else
            {
                return true;
            }
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

