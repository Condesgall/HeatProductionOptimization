public class AssetManager
{
    static ProductionUnit gasBoiler = new ProductionUnit("GB", 5.0, 500, 215, 1.1, 0);
    static ProductionUnit oilBoiler = new ProductionUnit("OB", 4.0, 700, 265, 1.2, 0);
    static ProductionUnit gasMotor = new ProductionUnit("GM", 3.6, 1100, 640, 1.9, 2.7);
    static ProductionUnit electricBoiler = new ProductionUnit("EK", 8.0, 50, 0, 0, -8.0);
    static List<ProductionUnit> productionUnits = new List<ProductionUnit>() {gasBoiler, oilBoiler, gasMotor, electricBoiler};   
    public static HeatingGrid Heatington {get; set; }= new HeatingGrid("Single District Heating Network", 1600, "Heatington", productionUnits);
}

public class HeatingGrid
{
    private string? Architecture;
    private int CityBuildings;
    private string? CityName;
    private List<ProductionUnit> ProductionUnits;

    public HeatingGrid(string architecture, int cityBuildings, string cityName, List<ProductionUnit> productionUnits)
    {
        Architecture = architecture;
        CityBuildings = cityBuildings;
        CityName = cityName;
        ProductionUnits = productionUnits;
    }

    public string? GetArchitecture
    {
        get {return Architecture; }
        set {Architecture = value;}
    }

    public int GetCityBuildings
    {
        get {return CityBuildings; }
        set {CityBuildings = value;}
    }

    public string? GetCityName
    {
        get {return CityName; }
        set {CityName = value;}
    }

    public List<ProductionUnit> GetProductionUnits
    {
        get {return ProductionUnits; }
        set {ProductionUnits = value;}
    }

}

public class ProductionUnit
{
    private string? Name;
    private double MaxHeat;
    private int ProductionCosts;
    private int CO2Emissions;
    private double GasConsumption;
    private double MaxElectricity;

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

    public string? GetName
    {
        get {return Name; }
        set {Name = value;}
    }

    public double GetMaxHeat
    {
        get {return MaxHeat; }
        set {MaxHeat = value;}
    }

    public int GetProductionCosts
    {
        get {return ProductionCosts; }
        set {ProductionCosts = value;}
    }

    public int GetCO2Emissions
    {
        get {return CO2Emissions; }
        set {CO2Emissions = value;}
    }

    public double GetGasConsumption
    {
        get {return GasConsumption; }
        set {GasConsumption = value;}
    }

    public double GetMaxElectricity
    {
        get {return MaxElectricity; }
        set {MaxElectricity = value;}
    }
}


