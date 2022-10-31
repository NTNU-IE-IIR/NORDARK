using System.Collections.Generic;

public class BiomeArea
{
    public string Biome;
    public string Name;
    public List<Vector2d> Coordinates;
    public AwesomeTechnologies.VegetationSystem.Biomes.BiomeMaskArea BiomeMaskArea;

    public BiomeArea()
    {
        Biome = "";
        Name = "";
        Coordinates = new List<Vector2d>();
        BiomeMaskArea = null;
    }
}
