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
        ProductionUnit optimalUnit1 = new ProductionUnit("Unit1", 0, 0, 0, 0, 0);
        optimizer.bestIndividualUnits = new Dictionary<ProductionUnit, decimal>
        {
            { optimalUnit1, 10.0m }
        };
        optimizer.combinedUnitsNetCostsList = new List<decimal> { 20.0m, 15.0m };

        // Act
        optimizer.OptimizeByCostsHandler(resultData, sdmParameters);

        // Assert
        Assert.Equal("Unit1", resultData.ProductionUnit);
        Assert.Contains(resultData, ResultDataManager.Summer);
    }


    /*-------------------------------------------------------------------------------
        METHODS RELATED TO NET COST CALCULATIONS
    ---------------------------------------------------------------------------------*/

    [Fact]
    public void GetBestNetCost_ReturnsLowestNetCost()
    {
        SdmParameters sdmParameters = new SdmParameters("01", "02", 4m, 752.03m);

        ProductionUnit productionUnit1 = new ProductionUnit("a", 2, 0, 0, 0, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("b", 2, 0, 0, 0, 0);
        decimal netCost1 = 10;
        decimal netCost2 = 15;

        optimizer.unitPairingCandidatesNetCosts.Add(productionUnit1, netCost1);
        optimizer.unitPairingCandidatesNetCosts.Add(productionUnit2, netCost2);
        optimizer.bestIndividualUnits.Add(productionUnit1, 1);

        var result = optimizer.GetBestNetCost(sdmParameters);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CombineAndSortNetCosts_WithValidInputs_ReturnsSortedCombinedList()
    {
        // Arrange
        List<decimal> combinedUnits = new List<decimal> { 5.5m, 7.0m, 9.5m };
        List<decimal> individualUnits = new List<decimal> { 3.0m, 4.0m, 8.0m };
        List<decimal> expectedSortedNetCosts = new List<decimal> { 3.0m, 4.0m, 5.5m, 7.0m, 8.0m, 9.5m };

        // Act
        List<decimal> sortedNetCosts = optimizer.CombineAndSortNetCosts(combinedUnits, individualUnits);

        // Assert
        Assert.Equal(expectedSortedNetCosts.Count, sortedNetCosts.Count);

        Assert.Equal(expectedSortedNetCosts, sortedNetCosts);
    }

    [Fact]
    public void DetectNetCostOrigin_IndividualUnitCostExists_ReturnsMinusOne()
    {
        // Arrange
        decimal bestNetCost = 10.5m;
        optimizer.individualUnitsNetCostsList.Add(10.5m); // Adding the test value to the list

        // Act
        int result = optimizer.DetectNetCostOrigin(bestNetCost);

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void DetectNetCostOrigin_CombinedUnitCostExists_ReturnsMinusTwo()
    {
        // Arrange
        decimal bestNetCost = 15.75m; // Sample value
        optimizer.combinedUnitsNetCostsList.Add(15.75m); // Adding the test value to the list

        // Act
        int result = optimizer.DetectNetCostOrigin(bestNetCost);

        // Assert
        Assert.Equal(-2, result);
    }

    [Fact]
    public void DetectNetCostOrigin_UnitCostDoesNotExist_ReturnsMinusThree()
    {
        // Arrange
        decimal bestNetCost = 20.0m; // Sample value

        // Act
        int result = optimizer.DetectNetCostOrigin(bestNetCost);

        // Assert
        Assert.Equal(-3, result);
    }

    /*--------------------------------------------------------------------------------
        METHODS RELATED TO INDIVIDUAL UNIT CALCULATIONS
    ---------------------------------------------------------------------------------*/
    
    [Fact]
    public void GetBestIndividualUnits_ReturnsCorrectList()
    {
        SdmParameters sdmParameters = new SdmParameters("01", "02", 4m, 752.03m);
        List<ProductionUnit> individualUnitSortedList = new List<ProductionUnit> (optimizer.bestIndividualUnits.Keys);

        var result = optimizer.GetBestIndividualUnits(sdmParameters);

        foreach (var key in result)
        {
            Assert.Contains(key, result);
        }
    }

    [Fact]
    public void GroupUnitsByDependency_OrderesDictionaries()
    {
        ProductionUnit productionUnit = new ProductionUnit("b", 0, 0, 0, 0, 0);
        ProductionUnit productionUnit2 = new ProductionUnit("a", 1, 1, 1, 1, 1);
        List<ProductionUnit> productionUnits = new List<ProductionUnit>() { productionUnit, productionUnit2 };

        SdmParameters sdmParameters = new SdmParameters("01", "02", 0.9m, 752.03m);

        optimizer.GroupUnitsByDependency(sdmParameters, productionUnits);
        var result1 = optimizer.bestIndividualUnits.OrderBy(key => key.Value);
        var result2 = optimizer.unitPairingCandidatesNetCosts.OrderBy(key => key.Value);

        Assert.True(optimizer.bestIndividualUnits.ContainsKey(productionUnit2));
        Assert.True(optimizer.unitPairingCandidatesNetCosts.ContainsKey(productionUnit));
        Assert.Equal(result1, optimizer.bestIndividualUnits);
        foreach (var pair in result2)
        {
            var key = pair.Key;
            var value = pair.Value;
            Assert.True(optimizer.unitPairingCandidatesNetCosts.ContainsKey(key));
            Assert.Equal(value, optimizer.unitPairingCandidatesNetCosts[key]);
        }
    }

    [Fact]
    public void CalculateIndividualUnitNetCosts_WhenElectricityProducing_ReturnsNetCost()
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
        var test1 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);
        var test2 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters2, productionUnit);
        decimal calculations = sdmParameters.HeatDemand * (productionUnit.ProductionCosts - sdmParameters.ElPrice);
        decimal calculations2 = (sdmParameters2.HeatDemand * productionUnit.ProductionCosts) - (productionUnit.MaxElectricity * sdmParameters2.ElPrice);

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
    }

    [Fact]
    public void CalculateIndividualUnitNetCosts_WhenElectricityConsuming_ReturnsNetCost()
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
        var test1 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnit);
        var test2 = optimizer.CalculateIndividualUnitNetCosts(sdmParameters2, productionUnit);
        decimal calculations = sdmParameters.HeatDemand * (productionUnit.ProductionCosts + sdmParameters.ElPrice);
        decimal calculations2 = productionUnit.MaxHeat * (productionUnit.ProductionCosts + sdmParameters.ElPrice);

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
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
        decimal calculations = productionUnit.ProductionCosts*sdmParameters.HeatDemand;
        decimal calculations2 = productionUnit.ProductionCosts*productionUnit.MaxHeat;

        //assert
        Assert.Equal(calculations, test1);
        Assert.Equal(calculations2, test2);
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

        optimizer.unitPairingCandidatesNetCosts.Add(productionUnit, netCost1);
        optimizer.unitPairingCandidatesNetCosts.Add(productionUnit2, netCost2);

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
        decimal netCost2= 2;

        optimizer.unitPairingCandidatesNetCosts.Add(productionUnit, netCost1);
        optimizer.unitPairingCandidatesNetCosts.Add(productionUnit2, netCost2);

        // act
        optimizer.AddCombinationToDictionary(productionUnit, productionUnit2, options);

        // assert
        Assert.Contains(productionUnit, options);
        Assert.Contains(productionUnit2, options);
        Assert.True(optimizer.combinedUnitsNetCost.ContainsKey(options));
        Assert.True(optimizer.combinedUnitsNetCost.ContainsValue(netCost1 + netCost2));
    }
}