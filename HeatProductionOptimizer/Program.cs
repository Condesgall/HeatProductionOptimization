// Loading is now fully functional, down below is a quick presentation:
// 1. Declare the file path, and two lists for 2 heating seasons (winter, summer)
// 2. Create a new ParameterLoader instance, input the file path as a parameter, Load the data
// 3. Data is accessible through <instancename>.Winter and <instancename>.Summer, corresponding to heating seasons
// 4. To check if it's loaded correctly, use <instancename>.Display();
// NOTE: TimeFrom and TimeTo are still in string format, for now i didn't see the reason to convert them

string filePath = "SourceData.csv";
ParameterLoader SourceData = new ParameterLoader(filePath);

Console.Clear();
SourceData.Load();
SourceData.Display();



