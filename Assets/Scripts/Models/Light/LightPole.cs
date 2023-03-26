using System.Collections.Generic;

public class LightPole
{
    public string Name { get; set; }
    public int ConfigurationIndex { get; set; }
    public List<string> Groups { get; set; }
    public Coordinate Coordinate { get; set; }
    public LightPrefab GameObject { get; set; }
    public string PrefabName { get; set; }
    public Location Location { get; set; }

    public LightPole(Location location)
    {
        Name = "";
        ConfigurationIndex = 0;
        Groups = new List<string>();
        Coordinate = new Coordinate();
        PrefabName = "";
        Location = location;
    }

    public LightPole(Coordinate coordinate, Location location, int configurationIndex, List<string> groups): this(location)
    {
        Coordinate = coordinate;
        ConfigurationIndex = configurationIndex;
        Groups = groups;
    }
}