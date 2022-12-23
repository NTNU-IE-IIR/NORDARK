using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class VegetationManager : MonoBehaviour, IObjectsManager
{
    private const string DEFAULT_BIOME = "Default";
    [SerializeField] private MapManager mapManager;
    [SerializeField] private VegetationControl vegetationControl;
    private AwesomeTechnologies.MeshTerrains.MeshTerrain meshTerrain;
    private List<BiomeArea> biomeAreas;
    private Dictionary<string, AwesomeTechnologies.VegetationSystem.BiomeType> biomes;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(vegetationControl);

        meshTerrain = GetComponent<AwesomeTechnologies.MeshTerrains.MeshTerrain>();
        biomeAreas = new List<BiomeArea>();
        biomes = new Dictionary<string, AwesomeTechnologies.VegetationSystem.BiomeType>() {
            {DEFAULT_BIOME, AwesomeTechnologies.VegetationSystem.BiomeType.Default},
            {"Uppsala forest", AwesomeTechnologies.VegetationSystem.BiomeType.Biome1},
            {"Alesund Forest", AwesomeTechnologies.VegetationSystem.BiomeType.Biome2},
        };
    }

    public void Create(Feature feature)
    {
        BiomeArea biomeArea = new BiomeArea();
        biomeArea.Biome = feature.Properties["biome"] as string;
        biomeArea.Name = feature.Properties["name"] as string;

        // In a GeoJSON polygon, the first and last points are be same
        for (int i=0; i<feature.Coordinates.Count-1; i++) {
            biomeArea.Coordinates.Add(new Vector2d(feature.Coordinates[i].x, feature.Coordinates[i].y));
        }

        AddBiomeArea(biomeArea);
    }

    public void Clear()
    {
        biomeAreas.Clear();
        vegetationControl.ClearBiomeAreas();
    }

    public void OnLocationChanged()
    {
        GenerateBiomes();
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();

        foreach (BiomeArea biomeArea in biomeAreas) {
            Feature feature = new Feature();
            feature.Properties.Add("type", "biomeArea");
            feature.Properties.Add("biome", biomeArea.Biome);
            feature.Properties.Add("name", biomeArea.Name);

            foreach (Vector2d coordinate in biomeArea.Coordinates) {
                feature.Coordinates.Add(new Vector3d(coordinate.y, coordinate.x, 0));
            }

            // In a GeoJSON polygon, the first and last points should be same
            if (biomeArea.Coordinates.Count > 0) {
                feature.Coordinates.Add(new Vector3d(biomeArea.Coordinates[0].y, biomeArea.Coordinates[0].x, 0));
            }

            features.Add(feature);
        }
        return features;
    }

    public void AddBiomeArea(BiomeArea biomeArea)
    {
        if (biomeArea.Name == "") {
            biomeArea.Name = Utils.DetermineNewName(biomeAreas.Select(biomeArea => biomeArea.Name).ToList(), "Biome");
        }
        biomeAreas.Add(biomeArea);
        vegetationControl.AddBiomeAreas(biomeArea.Name);
        GenerateBiomes();
    }

    public void DeleteSelectedBiomeArea()
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count) {
            biomeAreas[biomeAreaIndex].BiomeMaskArea.Destroy();
            biomeAreas.RemoveAt(biomeAreaIndex);
        }
    }

    public void GenerateBiomes()
    {
        meshTerrain.MeshTerrainMeshSourceList.Clear();
        foreach (Component component in gameObject.GetComponents<Component>()) {
            if (component is AwesomeTechnologies.VegetationSystem.Biomes.BiomeMaskArea) {
                Destroy(component);
            }
        }

        foreach(MeshRenderer tile in mapManager.GetTilesMeshRenderer()) {
            if(tile.gameObject.GetComponent<MeshFilter>() != null) {
                meshTerrain.MeshTerrainMeshSourceList.Add(new AwesomeTechnologies.MeshTerrains.MeshTerrainMeshSource() {
                    MeshRenderer = tile,
                    TerrainSourceID = AwesomeTechnologies.VegetationSystem.TerrainSourceID.TerrainSourceID1,
                    MaterialPropertyBlock = new MaterialPropertyBlock()
                });
            }
        }
        meshTerrain.GenerateMeshTerrain();

        foreach(BiomeArea biomeArea in biomeAreas) {
            biomeArea.BiomeMaskArea = gameObject.AddComponent<AwesomeTechnologies.VegetationSystem.Biomes.BiomeMaskArea>();
        
            if (vegetationControl.isVegetationDisplayed()) {
                if (!biomes.ContainsKey(biomeArea.Biome)) {
                    biomeArea.Biome = DEFAULT_BIOME;
                }
                biomeArea.BiomeMaskArea.BiomeType = biomes[biomeArea.Biome];
            } else {
                biomeArea.BiomeMaskArea.BiomeType = biomes[DEFAULT_BIOME];
            }
            
            biomeArea.BiomeMaskArea.ClearNodes();
            foreach(Vector2d coordinate in biomeArea.Coordinates) {
                AwesomeTechnologies.VegetationSystem.Biomes.Node node = new AwesomeTechnologies.VegetationSystem.Biomes.Node();
                node.Position = mapManager.GetUnityPositionFromCoordinates(new Vector3d(coordinate, 0));
                biomeArea.BiomeMaskArea.Nodes.Add(node);
            }
            biomeArea.BiomeMaskArea.PositionNodes();
        }
    }

    public List<string> GetBiomes()
    {
        return new List<string>(biomes.Keys);
    }

    public string GetBiomeOfCurrentBiomeArea()
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count) {
            return biomeAreas[biomeAreaIndex].Biome;
        } else {
            return DEFAULT_BIOME;
        }
    }

    public void ChangeBiome(string newBiome)
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count && biomeAreas[biomeAreaIndex].BiomeMaskArea != null) {
            biomeAreas[biomeAreaIndex].Biome = newBiome;

            if (vegetationControl.isVegetationDisplayed()) {
                biomeAreas[biomeAreaIndex].BiomeMaskArea.BiomeType = biomes[newBiome];
            } else {
                biomeAreas[biomeAreaIndex].BiomeMaskArea.BiomeType = biomes[DEFAULT_BIOME];
            }

            biomeAreas[biomeAreaIndex].BiomeMaskArea.PositionNodes();
        }
    }

    public void AddNodeToCurrentBiomeArea(Vector3 position)
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count) {
            Vector2d coordinate = new Vector2d(mapManager.GetCoordinatesFromUnityPosition(position));

            biomeAreas[biomeAreaIndex].Coordinates.Add(coordinate);

            // Remove default nodes
            if (biomeAreas[biomeAreaIndex].Coordinates.Count == 1) {
                biomeAreas[biomeAreaIndex].BiomeMaskArea.Nodes.Clear();
            }

            AwesomeTechnologies.VegetationSystem.Biomes.Node node = new AwesomeTechnologies.VegetationSystem.Biomes.Node();
            node.Position = mapManager.GetUnityPositionFromCoordinates(new Vector3d(coordinate, 0));
            biomeAreas[biomeAreaIndex].BiomeMaskArea.Nodes.Add(node);
            biomeAreas[biomeAreaIndex].BiomeMaskArea.PositionNodes();
        }
    }

    public void DeleteLastNodeOfCurrentBiomeArea()
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count) {
            if (biomeAreas[biomeAreaIndex].Coordinates.Count > 0) {
                biomeAreas[biomeAreaIndex].Coordinates.RemoveAt(biomeAreas[biomeAreaIndex].Coordinates.Count - 1);

                biomeAreas[biomeAreaIndex].BiomeMaskArea.Nodes.RemoveAt(biomeAreas[biomeAreaIndex].BiomeMaskArea.Nodes.Count -1);
                biomeAreas[biomeAreaIndex].BiomeMaskArea.PositionNodes();
            }
        }
    }

    public List<Vector2d> GetCoordinateOfCurrentBiomeArea()
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count) {
            return biomeAreas[biomeAreaIndex].Coordinates;
        } else {
            return new List<Vector2d>();
        }
    }
}
