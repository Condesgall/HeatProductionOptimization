using ResultDataManager_;
namespace HeatProductionOptimizer.Tests;
public class ResultDataManagerTests
{
    [Fact]

    public void OptimizationResults_WithValidParameters_SetsPropertiesCorrectly()
    {
        //arrange
        double producedHeat = 1;
        double producedElectricity = 2;
        double consumedElectricity = 3;
        double expenses = 3;
        double profit = 4;
        double primaryEnergyConsumption = 5;
        double co2Emissions = 6;

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
}