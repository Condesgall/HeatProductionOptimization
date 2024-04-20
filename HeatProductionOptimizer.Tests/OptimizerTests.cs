using AssetManager_;
using ResultDataManager_;
public class OptimizerTests
{
    public Optimizer optimizer = new Optimizer();

    [Fact]
    public void CalculateNetProductionCosts_WhenElectricityProducing_ReturnsNetCost()
    {
        //arrange
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 1.79m;
        decimal heatDemand2 = 3.00m;
        decimal elPrice = 752.03m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        SdmParameters sdmParameters2 = new SdmParameters(timeFrom, timeTo, heatDemand2, elPrice);
        ProductionUnit productionUnit = new ProductionUnit("GM", 3.6m, 1100, 640, 1.9m, 2.7m);
        //act
        var test1 = optimizer.CalculateNetProductionCosts(sdmParameters, productionUnit);
        var test2 = optimizer.CalculateNetProductionCosts(sdmParameters2, productionUnit);
        decimal calculations = sdmParameters.HeatDemand * (productionUnit.ProductionCosts - sdmParameters.ElPrice);
        decimal calculations2 = (sdmParameters2.HeatDemand * productionUnit.ProductionCosts) - (productionUnit.MaxElectricity * sdmParameters2.ElPrice);

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
    }

    [Fact]
    public void CalculateNetProductionCosts_WhenElectricityConsuming_ReturnsNetCost()
    {
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 1.79m;
        decimal heatDemand2 = 3.00m;
        decimal elPrice = 752.03m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        SdmParameters sdmParameters2 = new SdmParameters(timeFrom, timeTo, heatDemand2, elPrice);
        ProductionUnit productionUnit = new ProductionUnit("GM", 2m, 1100, 640, 1.9m, -8.0m);
        //act
        var test1 = optimizer.CalculateNetProductionCosts(sdmParameters, productionUnit);
        var test2 = optimizer.CalculateNetProductionCosts(sdmParameters2, productionUnit);
        decimal calculations = sdmParameters.HeatDemand * (productionUnit.ProductionCosts + sdmParameters.ElPrice);
        decimal calculations2 = productionUnit.MaxHeat * (productionUnit.ProductionCosts + sdmParameters.ElPrice);

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
    }

    [Fact]
    public void CalculateNetProductionCosts_WhenHeatBoiler_ReturnsNetCost()
    {
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 1.79m;
        decimal heatDemand2 = 3.00m;
        decimal elPrice = 752.03m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        SdmParameters sdmParameters2 = new SdmParameters(timeFrom, timeTo, heatDemand2, elPrice);
        ProductionUnit productionUnit = new ProductionUnit("GM", 2m, 1100, 640, 1.9m, 0);
        //act
        var test1 = optimizer.CalculateNetProductionCosts(sdmParameters, productionUnit);
        var test2 = optimizer.CalculateNetProductionCosts(sdmParameters2, productionUnit);
        decimal calculations = productionUnit.ProductionCosts*sdmParameters.HeatDemand;
        decimal calculations2 = productionUnit.ProductionCosts*productionUnit.MaxHeat;

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
    }

    [Fact]
    public void GetProductionUnitsNetCosts_AddsValuesToDiccAndSortsThem()
    {
        ProductionUnit productionUnit = new ProductionUnit("GB", 5.0m, 500, 215, 1.1m, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0);
        AssetManager.productionUnits = new List<ProductionUnit> {productionUnit, productionUnit2};
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 1.79m;
        decimal elPrice = 752.03m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        decimal netCost = optimizer.CalculateNetProductionCosts(sdmParameters, productionUnit);
        decimal netCost2 = optimizer.CalculateNetProductionCosts(sdmParameters, productionUnit2);

        optimizer.GetProductionUnitsNetCosts(sdmParameters, AssetManager.productionUnits);
        var sortedDicc = optimizer.individualUnitsNetCosts.OrderBy(key => key.Value);

        Assert.True(optimizer.individualUnitsNetCosts.ContainsKey(productionUnit));
        Assert.True(optimizer.individualUnitsNetCosts.ContainsKey(productionUnit2));
        Assert.True(optimizer.individualUnitsNetCosts.ContainsValue(netCost));
        Assert.True(optimizer.individualUnitsNetCosts.ContainsValue(netCost2));
        Assert.Equal(sortedDicc, optimizer.individualUnitsNetCosts);
    }

   [Fact]
    public void NetCostsWhenMoreThan1Unit_ReturnsDictionaryWithNetCosts()
    {
        ResultData resultData = new ResultData();
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 6.62m;
        decimal elPrice = 1190.94m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        Dictionary<ProductionUnit, decimal> individualUnitsOrdered = optimizer.GetProductionUnitsNetCosts(sdmParameters, AssetManager.productionUnits);
        List<ProductionUnit> unitSortedList = new List<ProductionUnit> (individualUnitsOrdered.Keys);
        List<ProductionUnit> options = new List<ProductionUnit>();
        int index = 0;
        int index2 = 1;
        ProductionUnit optimalUnit2 = unitSortedList[index2];
        ProductionUnit optimalUnit = unitSortedList[index];

        optimizer.NetCostsWhenMoreThan1Unit(sdmParameters, heatDemand, resultData, unitSortedList, options, index, index2);
        decimal netCosts1 = optimizer.CalculateNetProductionCosts(sdmParameters, optimalUnit);
        decimal netCosts2 = optimizer.CalculateNetProductionCosts(sdmParameters, optimalUnit2);
        decimal expectedValue = netCosts1 + netCosts2;

        Assert.True(optimizer.unitsAndNetCosts.ContainsKey(options));
        Assert.True(optimizer.unitsAndNetCosts.ContainsValue(expectedValue));
    }

    [Fact]
    public void OptimizeByCostsHandler_ReturnsMostOptimalOption()
    {
        ResultData resultData = new ResultData();
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 6.62m;
        decimal elPrice = 1190.94m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        Dictionary<ProductionUnit, decimal> individualUnitsOrdered = optimizer.GetProductionUnitsNetCosts(sdmParameters, AssetManager.productionUnits);
        List<ProductionUnit> unitSortedList = new List<ProductionUnit> (individualUnitsOrdered.Keys);
        Dictionary<ProductionUnit, decimal> unitsThatMeetDemand = new Dictionary<ProductionUnit, decimal>();
        foreach (var individualUnit in individualUnitsOrdered)
        {
            if (sdmParameters.HeatDemand <= individualUnit.Key.MaxHeat)
            {
                unitsThatMeetDemand.Add(individualUnit.Key, individualUnit.Value);
            }
        }

        optimizer.OptimizeByCostsHandler(resultData, sdmParameters, unitSortedList, unitsThatMeetDemand);

        Assert.Contains(resultData, ResultDataManager.Winter);
    }

    [Fact]
    public void UpdateResultData_UpdatesResultData()
    {
        ResultData resultData = new ResultData();
        ProductionUnit optimalUnit = new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0);
        ProductionUnit optimalUnit2 = new ProductionUnit("GB", 5.0m, 500, 215, 1.1m, 0);
        decimal netCost = 10m;
        decimal heatDemand = 9m;

        optimizer.UpdateResultData(resultData, optimalUnit, optimalUnit2, netCost, heatDemand);
        decimal gasConsumption = heatDemand * (optimalUnit.GasConsumption + optimalUnit2.GasConsumption);
        decimal co2Emissions = optimalUnit.MaxHeat * optimalUnit.Co2Emissions + (heatDemand - optimalUnit.MaxHeat) * optimalUnit2.Co2Emissions;
        string productionUnit = optimalUnit.Name + "," + optimalUnit2.Name;

        Assert.Equal(netCost, resultData.OptimizationResults.Profit);
        Assert.Equal(heatDemand, resultData.OptimizationResults.ProducedHeat);
        Assert.Equal(0, resultData.OptimizationResults.Expenses);
        Assert.Equal(gasConsumption, resultData.OptimizationResults.PrimaryEnergyConsumption);
        Assert.Equal(co2Emissions, resultData.OptimizationResults.Co2Emissions);
        Assert.Equal(productionUnit, resultData.ProductionUnit);
    }
}