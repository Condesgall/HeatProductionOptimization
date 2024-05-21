using System.Reflection;
using HeatingGridAvaloniaApp.Models;

/*public class OptimizerS1Tests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(4)]
    public void GetSeason_ReturnsSummerCorrectly(decimal inputtedHeatDemand)
    {
        var sdmParameters = new SdmParameters("01/01/1970 00:00", "01/01/1970 00:00", inputtedHeatDemand, 100);
        Optimizer optimizer = new Optimizer();
        string expected = "summer";
        string actual = optimizer.GetSeason(sdmParameters);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(4.01)]
    [InlineData(10)]
    [InlineData(20000)]
    [InlineData(6.0)]
    public void GetSeason_ReturnsWinterCorrectly(decimal inputtedHeatDemand)
    {
        var sdmParameters = new SdmParameters("01/01/1970 00:00", "01/01/1970 00:00", inputtedHeatDemand, 100);
        Optimizer optimizer = new Optimizer();
        string expected = "winter";
        string actual = optimizer.GetSeason(sdmParameters);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void OptimizeBy_SetsRightUnitPrimary()
    {
        ParameterLoader parameterLoader = new ParameterLoader("SourceData.csv");
        parameterLoader.Load();

        // In the first scenario, all 3 optimizations are better with the Gas Boiler, 
        // that is why it should always be the primary source
        // and so every result data should look the same.

        Optimizer optimizer1 = new Optimizer();
        Optimizer optimizer2 = new Optimizer();
        Optimizer optimizer3 = new Optimizer();

        
        optimizer1.OptimizeProduction(parameterLoader.ResultData, 1);
        var OptimizationByExpenses = ResultDataManager.Winter;

        optimizer2.OptimizeProduction(parameterLoader.Winter, 2);
        var OptimizationByCo2 = ResultDataManager.Winter;

        optimizer3.OptimizeProduction(parameterLoader.Winter, 3);
        var OptimizationByBoth = ResultDataManager.Winter;

        Assert.Equal(OptimizationByExpenses, OptimizationByCo2);
        Assert.Equal(OptimizationByExpenses, OptimizationByBoth);
        Assert.Equal(OptimizationByCo2, OptimizationByBoth);
    }
}
*/