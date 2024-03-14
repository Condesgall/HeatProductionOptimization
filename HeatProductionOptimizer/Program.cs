class Program
{
    static void Main()
    {
        AssetManagerJson assetManagerJson = new AssetManagerJson();
        assetManagerJson.SaveAMData(AssetManager.Heatington);
        assetManagerJson.LoadAMData();
    }
}
