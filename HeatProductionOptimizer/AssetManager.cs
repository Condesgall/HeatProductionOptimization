namespace AssetManager_
{
    public class AssetManager
    {
        //list of production units for the project
        public static List<ProductionUnit> productionUnits = new List<ProductionUnit>
        {
            new ProductionUnit("GB", 5.0, 500, 215, 1.1, 0),
            new ProductionUnit("OB", 4.0, 700, 265, 1.2, 0),
            new ProductionUnit("GM", 3.6, 1100, 640, 1.9, 2.7),
            new ProductionUnit("EK", 8.0, 50, 0, 0, -8.0)
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
        private double maxHeat;
        private int productionCosts;
        private int co2Emissions;
        private double gasConsumption;
        private double maxElectricity;

        //todo add image
        //constructor for the production unit class
        public ProductionUnit(string name_, double maxHeat_, int productionCosts_, int co2Emissions_, double gasConsumption_, double maxElectricity_)
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

        public double MaxHeat
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

        public double GasConsumption
        {
            get {return gasConsumption; }
            set {gasConsumption = value;}
        }

        public double MaxElectricity
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
