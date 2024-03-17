// Loading is now fully functional, down below is a quick presentation:
// 1. Declare the file path, and two lists for 2 heating seasons (winter, summer)
// 2. Create a new ParameterLoader instance, input the file path as a parameter, Load the data
// 3. Data is accessible through <instancename>.Winter and <instancename>.Summer, corresponding to heating seasons
// 4. Ive shown an example on how to display all of the data.
// NOTE: TimeFrom and TimeTo are still in string format, for now i didn't see the reason to convert them

// If you want to contribute with some more work, you can try and make commands to visualize all of the data
// in a nicer way, and outside of Program.cs.

string filePath = "SourceData.csv";

ParameterLoader SourceData = new ParameterLoader(filePath);
SourceData.Load();

Console.Clear();

Console.WriteLine("Winter data:");
Console.WriteLine();
foreach(SdmParameters param in SourceData.Winter)
{
    Console.WriteLine($"Time from: {param.TimeFrom}");
    Console.WriteLine($"Time to: {param.TimeTo}");
    Console.WriteLine($"Heat demand: {param.HeatDemand}");
    Console.WriteLine($"Electricity price: {param.ElPrice}");
    Console.WriteLine();
}

Console.WriteLine("Summer data:");
Console.WriteLine();
foreach(SdmParameters param in SourceData.Summer)
{
    Console.WriteLine($"Time from:{param.TimeFrom}");
    Console.WriteLine($"Time to:{param.TimeTo}");
    Console.WriteLine($"Heat demand:{param.HeatDemand}");
    Console.WriteLine($"Electricity price:{param.ElPrice}");
    Console.WriteLine();
}
