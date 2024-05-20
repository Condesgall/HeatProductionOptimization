using AssetManager_;
using ResultDataManager_;
public class OptimizerTests
{
    public Optimizer optimizer = new Optimizer();
    SdmParameters sdmParameters = new SdmParameters("01", "02", 1.79m, 752.03m);
    ResultData resultData = new ResultData();


    [Fact]
    public void OptimizeByCostsHandler_WhenIndividualUnit_UpdatesResultData()
    {
        SdmParameters sdmParameters = new SdmParameters("01", "02", 1m, 1m);
        ProductionUnit productionUnit = new ProductionUnit("", 1, 1, 1, 1, 1);
        AssetManager.productionUnits = new List<ProductionUnit>() { productionUnit };

        optimizer.OptimizeByCostsHandler(resultData, sdmParameters);

        Assert.Contains(resultData, ResultDataManager.Summer);
    }

    [Fact]
    public void GetOptimizedNetCosts_ReturnsLowestNetCost()
    {
        SdmParameters sdmParameters = new SdmParameters("01", "02", 4m, 752.03m);
        ProductionUnit productionUnit1 = new ProductionUnit("a", 4, 1, 0, 0, 0);
        AssetManager.productionUnits = new List<ProductionUnit>() { productionUnit1 };

        var result = optimizer.GetOptimizedNetCosts(sdmParameters);
        Dictionary<List<ProductionUnit>, decimal> expected = new Dictionary<List<ProductionUnit>, decimal>()
        {
            { AssetManager.productionUnits, 4}
        };

        Assert.Equal(expected.Values, result.Values);
        Assert.Equal(expected.Keys, result.Keys);
        
    }

    [Fact]
    public void GetOptimizedCO2_ReturnsList()
    {
        SdmParameters sdmParameters = new SdmParameters("01", "02", 4m, 752.03m);
        ProductionUnit productionUnit1 = new ProductionUnit("a", 4, 0, 4, 0, 0);
        AssetManager.productionUnits = new List<ProductionUnit>() { productionUnit1 };
        Co2AndNetCost expected = new Co2AndNetCost(AssetManager.productionUnits, 0, 16, 0);

        optimizer.GetOptimizedCO2(sdmParameters);

        foreach (var co2AndNetCost in optimizer.co2AndNetCostsCandidates)
        {
            Assert.Equal(expected.ProductionUnits, co2AndNetCost.ProductionUnits);
            Assert.Equal(expected.NetCost, co2AndNetCost.NetCost);
            Assert.Equal(expected.Co2Emissions, co2AndNetCost.Co2Emissions);
            Assert.Equal(expected.Result, co2AndNetCost.Result);
        }
    }

    [Fact]
    public void GetOptimizedCo2AndNet_ReturnsList()
    {
        ProductionUnit productionUnit = new ProductionUnit("a", 4, 0, 2, 0, 0);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit };
        SdmParameters sdmParameters1 = new SdmParameters("01", "02", 4m, 0);

        List<Co2AndNetCost> a = optimizer.GetOptimizedCo2AndNet(sdmParameters1, productionUnits);
        decimal expectedNetCost = optimizer.CalculateIndividualUnitNetCosts(sdmParameters1, productionUnit);
        decimal expectedCo2 = sdmParameters1.HeatDemand * productionUnit.Co2Emissions;
        decimal expectedResult = expectedCo2 * expectedNetCost;

        foreach (var item in a)
        {
            Assert.Equal(expectedNetCost, item.NetCost);
            Assert.Equal(expectedCo2, item.Co2Emissions);
            Assert.Equal(expectedResult, item.Result);
            Assert.Equal(productionUnits, item.ProductionUnits);
        }
    }

    /*--------------------------------------------------------------------------------
        METHODS RELATED TO INDIVIDUAL UNIT CALCULATIONS
    ---------------------------------------------------------------------------------*/

    [Fact]
    public void GroupUnitsByDependency_OrdersDictionaries()
    {
        ProductionUnit productionUnit = new ProductionUnit("b", 0.5m, 1, 0, 0, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("a", 1, 1, 1, 1, 1);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit, productionUnit2 };

        SdmParameters sdmParameters = new SdmParameters("01", "02", 1m, 752.03m);

        optimizer.GroupUnitsByDependency(sdmParameters, productionUnits);
        var result1 = optimizer.individualUnitCandidates.OrderBy(key => key.Value);
        var result2 = optimizer.unitPairingCandidates.OrderBy(key => key.Value);
        List<ProductionUnit> expectedList = new List<ProductionUnit>() { productionUnit2 };
        
        foreach (var key in optimizer.individualUnitCandidates.Keys)
        {
            Assert.Equal(key, expectedList);
        }
        Assert.Equal(result1, optimizer.individualUnitCandidates);
        foreach (var pair in result2)
        {
            var key = pair.Key;
            var value = pair.Value;
            Assert.True(optimizer.unitPairingCandidates.ContainsKey(key));
            Assert.Equal(value, optimizer.unitPairingCandidates[key]);
        }
    }

    [Fact]
    public void CalculateIndividualUnitNetCosts_WhenElectricityProducing_ReturnsNetCost()
    {
        //arrange
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 1.79m;
        decimal elPrice = 752.03m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        ProductionUnit productionUnit = new ProductionUnit("GM", 3.6m, 1100, 640, 1.9m, 2.7m);
        decimal electricityProduced = productionUnit.CalculateElectricityProduced(sdmParameters.HeatDemand);

        //act
        var test1 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);

        //when heatDemand <= maxEl -- heatDemand, sdmParameters
        decimal profit = electricityProduced * sdmParameters.ElPrice;
        decimal expenses = sdmParameters.HeatDemand * productionUnit.ProductionCosts;
        decimal result1 = expenses - profit;

        //assert
        Assert.Equal(result1, test1);
    }

    [Fact]
    public void CalculateIndividualUnitNetCosts_WhenElectricityConsuming_ReturnsNetCost()
    {
        string timeFrom = "00";
        string timeTo = "01";
        decimal heatDemand = 1.79m;
        decimal elPrice = 752.03m;
        SdmParameters sdmParameters = new SdmParameters(timeFrom, timeTo, heatDemand, elPrice);
        ProductionUnit productionUnit = new ProductionUnit("GM", 2m, 1100, 640, 1.9m, -8.0m);
        //act
        var test = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);

        decimal expenses = sdmParameters.HeatDemand * productionUnit.ProductionCosts;
        decimal extraExpenses = sdmParameters.HeatDemand * sdmParameters.ElPrice;
        decimal result = expenses + extraExpenses;

        //assert
        Assert.Equal(result, test);
    }

    [Fact]
    public void CalculateIndividualUnitNetCosts_WhenHeatBoiler_ReturnsNetCost()
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
        var test1 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);
        var test2 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters2, productionUnit);
        decimal calculations = productionUnit.ProductionCosts * sdmParameters.HeatDemand;
        decimal calculations2 = productionUnit.ProductionCosts * productionUnit.MaxHeat;

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
    }

    [Fact]
    public void CalculateCo2IndividualUnits_CalculatesCo2Emissions()
    {
        ProductionUnit productionUnit = new ProductionUnit("a", 2, 0, 2, 0, 0);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit };
        optimizer.individualUnitCandidates = new Dictionary<List<ProductionUnit>, decimal>
        {
            { productionUnits, 10 }
        };

        optimizer.CalculateCo2IndividualUnits(sdmParameters);
        decimal expectedCo2 = sdmParameters.HeatDemand * productionUnit.Co2Emissions;

        foreach (var item in optimizer.co2AndNetCostsCandidates)
        {
            Assert.Equal(10, item.NetCost);
            Assert.Equal(expectedCo2, item.Co2Emissions);
            Assert.Equal(productionUnits, item.ProductionUnits);
        }
    }

    /*------------------------------------------------------------------------------
        METHODS RELATED TO UNIT COMBINATIONS AND SELECTION
    --------------------------------------------------------------------------------*/

    [Fact]
    public void GetBestUnitCombinations_ReturnsDictionaryWithNetCosts()
    {
        // arrange
        ProductionUnit productionUnit = new ProductionUnit("b", 0, 0, 0, 0, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("a", 1, 1, 1, 1, 1);
        decimal netCost1 = 10;
        decimal netCost2 = 5;

        optimizer.unitPairingCandidates.Add(productionUnit, netCost1);
        optimizer.unitPairingCandidates.Add(productionUnit2, netCost2);

        SdmParameters sdmParameters = new SdmParameters("01", "02", 0.9m, 752.03m);

        int index1 = 0;
        int index2 = 1;

        // act
        var result = optimizer.GetBestUnitCombinations(sdmParameters, index1, index2);
        var orderedResult = result.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        // assert
        Assert.True(result.ContainsValue(netCost1 + netCost2));
        Assert.True(orderedResult.SequenceEqual(result));
    }

    [Fact]
    public void AddCombinationToDictionary_AddsCombinationToDicc()
    {
        // arrange
        List<ProductionUnit> options = new List<ProductionUnit>();
        ProductionUnit productionUnit = new ProductionUnit("b", 0, 0, 0, 0, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("a", 1, 1, 1, 1, 1);
        decimal netCost1 = 1;
        decimal netCost2 = 2;

        optimizer.unitPairingCandidates.Add(productionUnit, netCost1);
        optimizer.unitPairingCandidates.Add(productionUnit2, netCost2);

        // act
        optimizer.AddCombinationToDictionary(productionUnit, productionUnit2, options);

        // assert
        Assert.Contains(productionUnit, options);
        Assert.Contains(productionUnit2, options);
        Assert.True(optimizer.combinedUnitsNetCost.ContainsKey(options));
        Assert.True(optimizer.combinedUnitsNetCost.ContainsValue(netCost1 + netCost2));
    }

    [Fact]
    public void CalculateCo2CombinedUnits_CalculatesCo2Emissions()
    {
        ProductionUnit productionUnit = new ProductionUnit("a", 1, 0, 2, 0, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("b", 3, 0, 4, 0, 0);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit, productionUnit2 };
        SdmParameters sdmParameters = new SdmParameters("01", "02", 4m, 752.03m);

        optimizer.combinedUnitsNetCost = new Dictionary<List<ProductionUnit>, decimal>()
        {
            { productionUnits, 6 }
        };

        optimizer.CalculateCo2CombinedUnits(sdmParameters);
        decimal co2Emissions_Unit1 = productionUnit.MaxHeat * productionUnit.Co2Emissions;
        decimal co2Emissions_Unit2 = (sdmParameters.HeatDemand - productionUnit.MaxHeat) * productionUnit2.Co2Emissions;
        decimal expectedCo2 = co2Emissions_Unit1 + co2Emissions_Unit2;

        foreach (var item in optimizer.co2AndNetCostsCandidates)
        {
            Assert.Equal(6, item.NetCost);
            Assert.Equal(productionUnits, item.ProductionUnits);
            Assert.Equal(expectedCo2, item.Co2Emissions);
        }
    }
}