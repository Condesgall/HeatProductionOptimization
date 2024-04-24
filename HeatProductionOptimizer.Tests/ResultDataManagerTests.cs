using ResultDataManager_;
namespace HeatProductionOptimizer.Tests;
public class ResultDataManagerTests
{
    [Fact]

    public void OptimizationResults_WithValidParameters_SetsPropertiesCorrectly()
    {
        //arrange
        decimal producedHeat = 1;
        decimal producedElectricity = 2;
        decimal consumedElectricity = 3;
        decimal expenses = 3;
        decimal profit = 4;
        decimal primaryEnergyConsumption = 5;
        decimal co2Emissions = 6;

        //act
        OptimizationResults optimizationResults = new OptimizationResults(producedHeat, producedElectricity, consumedElectricity, expenses, profit, primaryEnergyConsumption, co2Emissions);

        //assert
        Assert.Equal(producedHeat, optimizationResults.ProducedHeat);
        Assert.Equal(producedElectricity, optimizationResults.ProducedElectricity);
        Assert.Equal(consumedElectricity, optimizationResults.ConsumedElectricity);
        Assert.Equal(expenses, optimizationResults.Expenses);
        Assert.Equal(profit, optimizationResults.Profit);
        Assert.Equal(primaryEnergyConsumption, optimizationResults.PrimaryEnergyConsumption);
        Assert.Equal(co2Emissions, optimizationResults.Co2Emissions);
    }
    
    [Fact]
    public void ResultData_WithValidParameters_SetsPropertiesCorrectly()
    {
        //arrange
        string timeFrom = "1";
        string timeTo = "2";
        string productionUnit = "unit";
        OptimizationResults optimizationResults = new OptimizationResults(0,0,0,0,0,0,0);

        //act
        ResultData resultData = new ResultData(timeFrom, timeTo, productionUnit, optimizationResults);
        //assert
        Assert.Equal(timeFrom, resultData.TimeFrom);
        Assert.Equal(timeTo, resultData.TimeTo);
        Assert.Equal(productionUnit, resultData.ProductionUnit);
        Assert.Equal(optimizationResults, resultData.OptimizationResults);
    }
}