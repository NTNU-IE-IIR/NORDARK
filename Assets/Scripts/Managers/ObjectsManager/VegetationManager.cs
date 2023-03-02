using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class VegetationManager : MonoBehaviour, IObjectsManager
{
    private const string DEFAULT_BIOME = "Default";
    [SerializeField] private MapManager mapManager;
    [SerializeField] private AwesomeTechnologies.VegetationSystem.VegetationSystemPro vegetationSystemPro;
    [SerializeField] private VegetationControl vegetationControl;
    private AwesomeTechnologies.MeshTerrains.MeshTerrain meshTerrain;
    private List<BiomeArea> biomeAreas;
    private Dictionary<string, AwesomeTechnologies.VegetationSystem.BiomeType> biomes;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(vegetationSystemPro);
        Assert.IsNotNull(vegetationControl);

        meshTerrain = GetComponent<AwesomeTechnologies.MeshTerrains.MeshTerrain>();
        biomeAreas = new List<BiomeArea>();
        biomes = new Dictionary<string, AwesomeTechnologies.VegetationSystem.BiomeType>() {
            {DEFAULT_BIOME, AwesomeTechnologies.VegetationSystem.BiomeType.Default},
            {"Uppsala forest", AwesomeTechnologies.VegetationSystem.BiomeType.Biome1},
            {"Alesund Forest", AwesomeTechnologies.VegetationSystem.BiomeType.Biome2},
        };
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Polygon")) {
            BiomeArea biomeArea = new BiomeArea();

            if (feature.Properties.ContainsKey("name")) {
                biomeArea.Name = feature.Properties["name"] as string;
            }

            if (feature.Properties.ContainsKey("biome")) {
                biomeArea.Biome = feature.Properties["biome"] as string;
            }

            GeoJSON.Net.Geometry.Polygon polygon = feature.Geometry as GeoJSON.Net.Geometry.Polygon;
            // In a GeoJSON polygon, the first and last points are be same
            for (int i=0; i<polygon.Coordinates[0].Coordinates.Count-1; i++) {
                GeoJSON.Net.Geometry.IPosition coordinate = polygon.Coordinates[0].Coordinates[i];
                biomeArea.Coordinates.Add(new Vector3d(coordinate.Latitude, coordinate.Longitude));
            }

            AddBiomeArea(biomeArea);
        }
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

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (BiomeArea biomeArea in biomeAreas) {
            
            List<List<List<double>>> coordinates = new List<List<List<double>>> {new List<List<double>>()};
            foreach (Vector3d coordinate in biomeArea.Coordinates) {
                coordinates[0].Add(new List<double>{coordinate.longitude, coordinate.latitude, 0});
            }
            // In a GeoJSON polygon, the first and last points should be same
            if (biomeArea.Coordinates.Count > 0) {
                coordinates[0].Add(new List<double>{biomeArea.Coordinates[0].longitude, biomeArea.Coordinates[0].latitude, 0});
            }
            GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Polygon(coordinates);

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("type", "biomeArea");
            properties.Add("biome", biomeArea.Biome);
            properties.Add("name", biomeArea.Name);

            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
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

        List<Tile> tiles = mapManager.GetTiles();
        foreach(Tile tile in tiles) {
            if (tile.MeshFilter != null) {
                meshTerrain.MeshTerrainMeshSourceList.Add(new AwesomeTechnologies.MeshTerrains.MeshTerrainMeshSource() {
                    MeshRenderer = tile.MeshRenderer,
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
            foreach(Vector3d coordinate in biomeArea.Coordinates) {
                AwesomeTechnologies.VegetationSystem.Biomes.Node node = new AwesomeTechnologies.VegetationSystem.Biomes.Node();
                node.Position = mapManager.GetUnityPositionFromCoordinates(coordinate);
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
            Vector3d coordinate = mapManager.GetCoordinatesFromUnityPosition(position);

            biomeAreas[biomeAreaIndex].Coordinates.Add(coordinate);

            // Remove default nodes
            if (biomeAreas[biomeAreaIndex].Coordinates.Count == 1) {
                biomeAreas[biomeAreaIndex].BiomeMaskArea.Nodes.Clear();
            }

            AwesomeTechnologies.VegetationSystem.Biomes.Node node = new AwesomeTechnologies.VegetationSystem.Biomes.Node();
            node.Position = mapManager.GetUnityPositionFromCoordinates(coordinate);
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

    public List<Vector3d> GetCoordinateOfCurrentBiomeArea()
    {
        int biomeAreaIndex = vegetationControl.GetBiomeAreaIndex();
        if (biomeAreaIndex > -1 && biomeAreaIndex < biomeAreas.Count) {
            return biomeAreas[biomeAreaIndex].Coordinates;
        } else {
            return new List<Vector3d>();
        }
    }

    public void AddCamera(Camera camera)
    {
        vegetationSystemPro.AddCamera(camera, false, true, false);
    }

    public void RemoveCamera(Camera camera)
    {
        vegetationSystemPro.RemoveCamera(camera);
    }
}