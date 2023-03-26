using System.Collections.Generic;

public class BiomeArea
{
    public string Biome;
    public string Name;
    public List<Coordinate> Coordinates;
    public AwesomeTechnologies.VegetationSystem.Biomes.BiomeMaskArea BiomeMaskArea;
    public Location Location;

    public BiomeArea(Location location)
    {
        Biome = "";
        Name = "";
        Coordinates = new List<Coordinate>();
        BiomeMaskArea = null;
        Location = location;
    }
}
