using HeatingGridAvaloniaApp.Models;
namespace HeatProductionOptimizer.Test;
public class ResultDataManagerTests
{
    ResultData resultData = new ResultData();
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

    [Fact]
    public void UpdateOptimizationResults_NetCosts_Expenses()
    {
        resultData.UpdateOptimizationResults_NetCosts(100);
        Assert.Equal(100, resultData.OptimizationResults.Expenses);
    }

    [Fact]
    public void UpdateOptimizationResults_NetCosts_Profit()
    {
        resultData.UpdateOptimizationResults_NetCosts(-50);
        Assert.Equal(50, resultData.OptimizationResults.Profit);
    }

    [Fact]
    public void UpdateOptimizationResults_PrimaryEnergyConsumption_ElectricBoiler()
    {
        ProductionUnit optimalUnit = new ProductionUnit("", 1, 2, 3, 0, 8);
        ProductionUnit secondUnit = new ProductionUnit("", 0, 0, 0, 0, 0);
        SdmParameters sdmParameters = new SdmParameters("", "", 1, 2);
        
        resultData.UpdateOptimizationResults_PrimaryEnergyConsumption(optimalUnit, secondUnit, sdmParameters);
        
        Assert.Equal(1, resultData.OptimizationResults.PrimaryEnergyConsumption);
    }

    [Fact]
    public void UpdateOptimizationResults_PrimaryEnergyConsumption_Gas()
    {
        ProductionUnit optimalUnit = new ProductionUnit("", 1, 2, 3, 1, 3);
        ProductionUnit secondUnit = new ProductionUnit("", 0, 0, 0, 2, 0);
        SdmParameters sdmParameters = new SdmParameters("", "", 1, 2);

        decimal gasConsumption = optimalUnit.MaxHeat * optimalUnit.GasConsumption + (sdmParameters.HeatDemand - optimalUnit.MaxHeat) * secondUnit.GasConsumption;
        
        resultData.UpdateOptimizationResults_PrimaryEnergyConsumption(optimalUnit, secondUnit, sdmParameters);
        
        Assert.Equal(gasConsumption, resultData.OptimizationResults.PrimaryEnergyConsumption);
    }

    [Fact]
    public void UpdateOptimizationResults_Co2Emissions_OptimalUnitCanReachHeatDemand()
    {
        SdmParameters sdmParameters = new SdmParameters("", "", 1, 2);
        ProductionUnit optimalUnit = new ProductionUnit("", 1, 2, 3, 0, 3);
        ProductionUnit Unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);

        resultData.UpdateOptimizationResults_Co2Emissions(optimalUnit, Unit2, sdmParameters);

        Assert.Equal(3, resultData.OptimizationResults.Co2Emissions);
    }

    [Fact]
    public void UpdateOptimizationResults_Co2Emissions_OptimalUnitCantReachHeatDemand()
    {
        SdmParameters sdmParameters = new SdmParameters("", "", 1, 2);
        ProductionUnit optimalUnit = new ProductionUnit("", 1, 2, 3, 0, 3);
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        var expected = optimalUnit.MaxHeat * optimalUnit.Co2Emissions + (sdmParameters.HeatDemand - optimalUnit.MaxHeat) * unit2.Co2Emissions;

        resultData.UpdateOptimizationResults_Co2Emissions(optimalUnit, unit2, sdmParameters);

        Assert.Equal(expected, resultData.OptimizationResults.Co2Emissions);
    }

    [Fact]
    public void UpdateResultData_Name_TwoUnits()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 1, 2, 3, 0, 3);
        ProductionUnit unit2 = new ProductionUnit("b", 3, 4, 1, 0, 0);
        
        resultData.UpdateResultData_Name(optimalUnit, unit2);
        
        Assert.Equal("a+b", resultData.ProductionUnit);
    }

    [Fact]
    public void UpdateResultData_Name_OneUnit()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 1, 2, 3, 0, 3);
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        
        resultData.UpdateResultData_Name(optimalUnit, unit2);
        
        Assert.Equal("a", resultData.ProductionUnit);
    }

    [Fact]
    public void UpdateOptimizationResults_Electricity_WhenMinus1()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 4, 2, 3, 0, 3);
        ProductionUnit unit2 = new ProductionUnit("", 8, 0, 0, 0, -8);
        SdmParameters sdmParameters = new SdmParameters("", "", 6, 2);
        decimal heatRemaining = sdmParameters.HeatDemand - optimalUnit.MaxHeat;

        resultData.UpdateOptimizationResults_Electricity(optimalUnit, unit2, sdmParameters);

        decimal electricityProduced = optimalUnit.CalculateElectricityProduced(sdmParameters.HeatDemand);
        decimal electricityConsumed = unit2.CalculateElectricityConsumed(heatRemaining);
        decimal result = electricityProduced - electricityConsumed;

        if (result>0)
        {
            Assert.Equal(result, resultData.OptimizationResults.ProducedElectricity); 
        }
        else
        {
            Assert.Equal(result, resultData.OptimizationResults.ConsumedElectricity); 
        }
    }

    [Fact]
    public void UpdateOptimizationResults_Electricity_WhenMinus2()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 4, 2, 3, 0, 2);
        ProductionUnit unit2 = new ProductionUnit("", 8, 0, 0, 0, 0);
        SdmParameters sdmParameters = new SdmParameters("", "", 6, 2);
        
        var expected = optimalUnit.MaxElectricity;
        resultData.UpdateOptimizationResults_Electricity(optimalUnit, unit2, sdmParameters);

        Assert.Equal(expected, resultData.OptimizationResults.ProducedElectricity);
    }

    [Fact]
    public void UpdateOptimizationResults_Electricity_WhenMinus3()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 4, 2, 3, 0, 0);
        ProductionUnit unit2 = new ProductionUnit("", 8, 0, 0, 0, 3);
        SdmParameters sdmParameters = new SdmParameters("", "", 6, 2);
        //heat remaining = 6 - 4 = 2
        decimal heatRemaining = sdmParameters.HeatDemand - optimalUnit.MaxHeat;
        
        var expected = unit2.CalculateElectricityProduced(heatRemaining);
        resultData.UpdateOptimizationResults_Electricity(optimalUnit, unit2, sdmParameters);

        Assert.Equal(expected, resultData.OptimizationResults.ProducedElectricity);
    }

    [Fact]
    public void UpdateOptimizationResults_Electricity_WhenMinus4()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 4, 2, 3, 0, 0);
        ProductionUnit unit2 = new ProductionUnit("", 8, 0, 0, 0, -3);
        SdmParameters sdmParameters = new SdmParameters("", "", 6, 2);
        decimal heatRemaining = sdmParameters.HeatDemand - optimalUnit.MaxHeat;

        var expected = unit2.CalculateElectricityConsumed(heatRemaining);
        resultData.UpdateOptimizationResults_Electricity(optimalUnit, unit2, sdmParameters);

        Assert.Equal(expected, resultData.OptimizationResults.ConsumedElectricity);
    }

    [Fact]
    public void UpdateOptimizationResults_Electricity_WhenMinus5()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 4, 2, 3, 0, 6);
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        SdmParameters sdmParameters = new SdmParameters("", "", 6, 2);
        
        var expected = optimalUnit.CalculateElectricityProduced(sdmParameters.HeatDemand);
        resultData.UpdateOptimizationResults_Electricity(optimalUnit, unit2, sdmParameters);

        Assert.Equal(expected, resultData.OptimizationResults.ProducedElectricity);
    }

    [Fact]
    public void UpdateOptimizationResults_Electricity_WhenMinus6()
    {
        ProductionUnit optimalUnit = new ProductionUnit("a", 4, 2, 3, 0, 6);
        ProductionUnit unit2 = new ProductionUnit("", 0, 0, 0, 0, 0);
        SdmParameters sdmParameters = new SdmParameters("", "", 6, 2);
        
        var expected = optimalUnit.CalculateElectricityConsumed(sdmParameters.HeatDemand);
        resultData.UpdateOptimizationResults_Electricity(optimalUnit, unit2, sdmParameters);

        Assert.Equal(expected, resultData.OptimizationResults.ProducedElectricity);
    }
}