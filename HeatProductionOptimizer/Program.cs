using AssetManager_;
using ResultDataManager_;
using ResultDataStorage;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
//using Avalonia.ReactiveUI;
using HeatingGridAvaloniaApp.Views;
using System;
using System.IO;
using HeatingGridAvaloniaApp;
using Avalonia.Win32;
using Avalonia.UsePlatformDetect;

class Program
{
    public static bool running = true;
    static string fullPath = Path.GetFullPath("SourceData.csv");
    public static ParameterLoader parameterLoader = new ParameterLoader(fullPath);
    public static AssetManager assetManager = new AssetManager();
    public static  ResultDataManager resultDataManager = new ResultDataManager();
    public static ResultDataCSV resultDataCSV = new ResultDataCSV("ResultData.csv");
    static Optimizer optimizer = new Optimizer();
    
    [STAThread]
    static void Main(string[] args)
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        ResultData gbResults = new ResultData();
        gbResults.ProductionUnit = "GB";
        gbResults.OptimizationResults.ProducedHeat = 1m;
        gbResults.OptimizationResults.ProducedElectricity = 2m;
        gbResults.OptimizationResults.ConsumedElectricity = 3m;
        gbResults.OptimizationResults.Expenses = 4m;
        gbResults.OptimizationResults.Profit = 5m;
        gbResults.OptimizationResults.PrimaryEnergyConsumption = 6m;
        gbResults.OptimizationResults.Co2Emissions = 7m;
        List<ResultData> resultData = new List<ResultData>() { gbResults };
        resultDataCSV.Save(resultData);
        resultDataCSV.loadedResultData = new List<ResultData>();
        resultDataCSV.Load();
        foreach (var data in resultDataCSV.loadedResultData)
        {
            Console.WriteLine(data.ProductionUnit);
            Console.WriteLine(data.OptimizationResults.ProducedElectricity);
        }

        Console.WriteLine("Welcome to heat production optimization.");
        Commands();
    }

    private static void Commands()
    {
        while (running == true)
        {
            Console.WriteLine("");
            Console.WriteLine("Select your option: ");
            Console.Write("");
            Console.WriteLine("1. Assets");
            Console.WriteLine("2. Source Data");
            Console.WriteLine("3. Result Data");
            Console.WriteLine("4. Exit");
            Console.Write("> ");

            string? userInput = Console.ReadLine();
            int userOption = CheckIfValidInput(userInput, 4);

            switch (userOption)
            {
                case 1:
                    DiplayAssetManagerHandler();
                    break;

                case 2:
                    DisplaySourceDataManagerHandler();
                    break;

                case 3:
                    DisplayResultDataManagerHandler();
                    break;

                case 4:
                    running = false;
                    break;

                default:
                    Console.WriteLine("Select an option.");
                    break;
            }
        }
    }

    private static void DiplayAssetManagerHandler()
    {
        assetManager.DisplayAssetManager();
    }

    //todo: figure out a way to separate the data to display
    private static void DisplaySourceDataManagerHandler()
    {
        parameterLoader.Load();
        Console.WriteLine("1. Winter data");
        Console.WriteLine("2. Summer data");
        Console.Write("> ");
        string? userInput = Console.ReadLine();
        int userChoice = CheckIfValidInput(userInput, 2);

        if (userChoice == 1)
        {
            parameterLoader.DisplayWinterData();
        }
        if (userChoice == 2)
        {
            parameterLoader.DisplaySummerData();
        }
    }

    //to do
    private static void DisplayResultDataManagerHandler()
    {
        Console.WriteLine("1. Summer result data.");
        Console.WriteLine("2. Winter result data.");
        Console.Write("> ");
        string? userInput = Console.ReadLine();
        int userChoice = CheckIfValidInput(userInput, 2);
        if (userChoice == 1)
        {
            resultDataManager.DisplayResultData(ResultDataManager.Summer);
            resultDataCSV.Save(ResultDataManager.Summer);
        }
        if (userChoice == 2)
        {
            resultDataManager.DisplayResultData(ResultDataManager.Winter);
            resultDataCSV.Save(ResultDataManager.Winter);
        }
    }

    private static int CheckIfValidInput(string? userInput, int numberOfOptions)
    {
        int userInput_int;
        while (!int.TryParse(userInput, out userInput_int) || string.IsNullOrEmpty(userInput) || userInput_int > numberOfOptions)
        {
            Console.WriteLine("That option is not valid.");
            Console.Write("> ");
        }
        return userInput_int;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { AllowEglInitialization = true })
            .LogToTrace()
            .UseReactiveUI();
    }
}
