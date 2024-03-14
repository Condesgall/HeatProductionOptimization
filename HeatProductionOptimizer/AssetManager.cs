public class AssetManager
{
    static ProductionUnit gasBoiler = new ProductionUnit("GB", 5.0, 500, 215, 1.1, 0);
    static ProductionUnit oilBoiler = new ProductionUnit("OB", 4.0, 700, 265, 1.2, 0);
    static ProductionUnit gasMotor = new ProductionUnit("GM", 3.6, 1100, 640, 1.9, 2.7);
    static ProductionUnit electricBoiler = new ProductionUnit("EK", 8.0, 50, 0, 0, -8.0);
    static HeatingGrid city = new HeatingGrid("Single District Heating Network", 1600, "Heatington");
}

public class HeatingGrid
{
    private string Architecture;
    private int CityBuildings;
    private string CityName;

    public HeatingGrid(string architecture, int cityBuildings, string cityName)
    {
        Architecture = architecture;
        CityBuildings = cityBuildings;
        CityName = cityName;
    }

    public string GetArchitecture()
    {
        return Architecture;
    }
    public int GetCityBuildings()
    {
        return CityBuildings;
    }
    public string GetCityName()
    {
        return CityName;
    }
}

public class ProductionUnit
{
    private string Name;
    private double MaxHeat;
    private int ProductionCosts;
    private int CO2Emissions;
    private double GasConsumption;
    private double MaxElectricity;
    //private string GraphicalRepresentationFilePath; // path to the image

    //todo add image
    public ProductionUnit(string name, double maxHeat, int productionCosts, int co2Emissions, double gasConsumption, double maxElectricity)
    {
        Name = name;
        MaxHeat = maxHeat;
        ProductionCosts = productionCosts;
        CO2Emissions = co2Emissions;
        GasConsumption = gasConsumption;
        MaxElectricity = maxElectricity;
    }

    public string GetName()
    {
        return Name;
    }

    public double GetMaxHeat()
    {
        return MaxHeat;
    }

    public int GetProductionCosts()
    {
        return ProductionCosts;
    }

    public int GetCO2Emissions()
    {
        return CO2Emissions;
    }

    public double GetGasConsumption()
    {
        return GasConsumption;
    }

    public double GetMaxElectricity()
    {
        return MaxElectricity;
    }
}
