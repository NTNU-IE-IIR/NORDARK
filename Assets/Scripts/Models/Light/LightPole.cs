using System.Collections.Generic;

public class LightPole
{
    public Coordinate Coordinate { get; set; }
    public string Name { get; set; }
    public LightPrefab Light { get; set; }
    public string PrefabName { get; set; }
    public int ConfigurationIndex { get; set; }
    public List<string> Groups { get; set; }

    public LightPole()
    {
        Coordinate = new Coordinate();
        Name = "";
        PrefabName = "";
        ConfigurationIndex = 0;
        Groups = new List<string>();
    }

    public LightPole(Coordinate coordinate, int configurationIndex, List<string> groups): base()
    {
        Coordinate = coordinate;
        ConfigurationIndex = configurationIndex;
        Groups = groups;
    }
}