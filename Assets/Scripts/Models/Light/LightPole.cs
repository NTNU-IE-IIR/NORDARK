public class LightPole
{
    public Vector3d Coordinates { get; set; }
    public string Name { get; set; }
    public LightPrefab Light { get; set; }
    public string PrefabName { get; set; }
    public int ConfigurationIndex { get; set; }

    public LightPole()
    {
        Coordinates = new Vector3d();
        Name = "";
        PrefabName = "";
        ConfigurationIndex = 0;
    }

    public LightPole(Vector3d coordinates, int configurationIndex): base()
    {
        Coordinates = coordinates;
        ConfigurationIndex = configurationIndex;
    }
}