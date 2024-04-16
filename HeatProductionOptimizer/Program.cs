using AssetManager_;

class Program
{
    public static bool running = true;
    public static ParameterLoader sourceDataManager = new ParameterLoader("C:\\Users\\ritab\\Downloads\\University\\Semester Project\\code\\HeatProductionOptimization\\HeatProductionOptimizer\\SourceData.csv");
    public static AssetManager assetManager = new AssetManager();
    static void Main()
    {
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

    private static void DisplaySourceDataManagerHandler()
    {
        sourceDataManager.Load();
        sourceDataManager.Display();
    }

    private static void DisplayResultDataManagerHandler()
    {

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
}
