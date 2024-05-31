using HeatingGridAvaloniaApp.Models;
public class OptimizerTests
{
    public Optimizer optimizer = new Optimizer();
    SdmParameters sdmParameters = new SdmParameters("01", "02", 1.79m, 752.03m);
    ResultData resultData = new ResultData();

    [Fact]
    public void SaveResult_SavesToResultData()
    {
        ResultDataManager.ResultData.Clear();
        List<ProductionUnit> productionUnits = new List<ProductionUnit>()
        {
            new ProductionUnit("GB", 5.0m, 500, 215, 1.1m, 0), //heat only boiler
            new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0), //heat only boiler
        };

        SortedSet<Co2AndNetCost> sortedUnits = new SortedSet<Co2AndNetCost>()
        {
            new Co2AndNetCost(productionUnits, 10, 10, 10),
        };

        ResultData newResultData = new ResultData();
        newResultData.UpdateResultData(productionUnits, 10, sdmParameters);

        optimizer.SaveResult(sdmParameters, sortedUnits);

        ResultData actual = ResultDataManager.ResultData.First();

        Assert.Equal(newResultData.OptimizationResults.Co2Emissions, actual.OptimizationResults.Co2Emissions);
        Assert.Equal(newResultData.OptimizationResults.Profit, actual.OptimizationResults.Profit);
        Assert.Equal(newResultData.OptimizationResults.Expenses, actual.OptimizationResults.Expenses);
        Assert.Equal(newResultData.OptimizationResults.ConsumedElectricity, actual.OptimizationResults.ConsumedElectricity);
        Assert.Equal(newResultData.OptimizationResults.ProducedElectricity, actual.OptimizationResults.ProducedElectricity);
        Assert.Equal(newResultData.ProductionUnit, actual.ProductionUnit);
    }

    [Fact]
    public void GroupUnitsByDependency_UpdatesLists()
    {
        List<ProductionUnit> productionUnits = new List<ProductionUnit>()
        {
            new ProductionUnit("GB", 1m, 500, 215, 1.1m, 0),
            new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0),
        };

        optimizer.GroupUnitsByDependency(sdmParameters, productionUnits);

        Assert.Contains(productionUnits.Last(), optimizer.individualUnitCandidates);
        Assert.Contains(productionUnits.Last(), optimizer.unitPairingCandidates);
        Assert.Contains(productionUnits.First(), optimizer.unitPairingCandidates);
    }

    [Fact]
    public void CalculateCo2AndNet_WhenUnitCombination_AddsToList()
    {
        List<ProductionUnit> productionUnits = new List<ProductionUnit>()
        {
            new ProductionUnit("GB", 1m, 500, 215, 1.1m, 0),
            new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0),
        };
        decimal heatProducedUnit1 = productionUnits.First().MaxHeat;
        decimal heatProducedUnit2 = sdmParameters.HeatDemand - heatProducedUnit1;

        decimal co2EmissionsUnit1 = heatProducedUnit1 * productionUnits.First().Co2Emissions;
        decimal co2EmissionsUnit2 = heatProducedUnit2 * productionUnits.Last().Co2Emissions;
        decimal expectedCo2 = co2EmissionsUnit1 + co2EmissionsUnit2;

        decimal expectedNetCost = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnits, heatProducedUnit1);

        decimal expectedResult = (optimizer.Co2Weight * expectedCo2) + (expectedNetCost * optimizer.NetWeight);

        Co2AndNetCost expected = new Co2AndNetCost(productionUnits, expectedNetCost, expectedCo2, expectedResult);

        optimizer.CalculateCo2AndNet(sdmParameters, productionUnits);

        Co2AndNetCost actual = optimizer.unitCandidates.First();   

        Assert.Equal(expected.NetCost, actual.NetCost); 
        Assert.Equal(expected.Co2Emissions, actual.Co2Emissions); 
        Assert.Equal(expected.Result, actual.Result); 
        Assert.Equal(expected.ProductionUnits, actual.ProductionUnits); 
    }

    [Fact]
    public void CalculateCo2AndNet_WhenIndividualUnit_AddsToList()
    {
        List<ProductionUnit> productionUnits = new List<ProductionUnit>()
        {
            new ProductionUnit("GB", 1m, 500, 215, 1.1m, 0),
        };

        decimal heatProducedUnit1 = sdmParameters.HeatDemand;
        decimal expectedCo2 = heatProducedUnit1 * productionUnits.First().Co2Emissions;
        decimal expectedNetCost = optimizer.CalculateIndividualUnitNetCosts(sdmParameters, productionUnits, heatProducedUnit1);
        decimal expectedResult = (optimizer.Co2Weight * expectedCo2) + (expectedNetCost * optimizer.NetWeight);
        Co2AndNetCost expected = new Co2AndNetCost(productionUnits, expectedNetCost, expectedCo2, expectedResult);

        optimizer.CalculateCo2AndNet(sdmParameters, productionUnits);

        Co2AndNetCost actual = optimizer.unitCandidates.First();   

        Assert.Equal(expected.NetCost, actual.NetCost); 
        Assert.Equal(expected.Co2Emissions, actual.Co2Emissions); 
        Assert.Equal(expected.Result, actual.Result); 
        Assert.Equal(expected.ProductionUnits, actual.ProductionUnits); 
    }

    [Fact]
    public void GetBestUnitCombinations()
    {
        optimizer.unitPairingCandidates = new List<ProductionUnit>()
        {
            new ProductionUnit("GB", 1m, 500, 215, 1.1m, 0),
            new ProductionUnit("OB", 4.0m, 700, 265, 1.2m, 0),
        };

        decimal totalNetCost = 0;
        decimal totalCo2 = 0;
        optimizer.CalculateCo2AndNetUnitCombination(sdmParameters, optimizer.unitPairingCandidates, ref totalCo2, ref totalNetCost);
        decimal result = (optimizer.Co2Weight * totalCo2) + (totalNetCost * optimizer.NetWeight);
        Co2AndNetCost expected = new Co2AndNetCost(optimizer.unitPairingCandidates, totalNetCost, totalCo2, result);

        optimizer.GetBestUnitCombinations(sdmParameters, 0, 1);

        Co2AndNetCost actual = optimizer.unitCandidates.First();

        Assert.Equal(expected.NetCost, actual.NetCost);
        Assert.Equal(expected.Co2Emissions, actual.Co2Emissions);
        Assert.Equal(expected.ProductionUnits, actual.ProductionUnits);
        Assert.Equal(expected.Result, actual.Result);
    }   
}