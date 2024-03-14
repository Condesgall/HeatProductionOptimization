using Newtonsoft.Json;
public interface IAssetManagerJson
{
    abstract void SaveAMData(HeatingGrid city);
    abstract HeatingGrid LoadAMData();
}

public class AssetManagerJson : IAssetManagerJson
{
    public void SaveAMData(HeatingGrid heatingArea)
    {
        string heatingGridJson = JsonConvert.SerializeObject(heatingArea, Formatting.Indented);
        File.WriteAllText("heatingGrid.json", heatingGridJson);
    }

    public HeatingGrid LoadAMData()
    {
        try
        {
            if (File.Exists("heatingGrid.json"))
            {
                string dataInJson = File.ReadAllText("heatingGrid.json");
                HeatingGrid? deserializedFile = JsonConvert.DeserializeObject<HeatingGrid>(dataInJson);
                if (deserializedFile != null)
                {
                    return deserializedFile;
                }
                else
                {
                    HeatingGrid newHeatingGrid = new HeatingGrid("", 0, "", new List < ProductionUnit >());
                    return newHeatingGrid;
                }
            }
            else
            {
                File.Create("heatingGrid.json").Close();
                HeatingGrid newHeatingGrid = new HeatingGrid("", 0, "", new List < ProductionUnit >());
                return newHeatingGrid;
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File was not found: {ex.Message}");
            throw;
        }

        catch (JsonException)
        {
            Console.WriteLine("Couldn't deserialize the file.");
            throw;
        }

        catch (Exception)
        {
            Console.WriteLine("Error");
            throw;
        }
    }
}