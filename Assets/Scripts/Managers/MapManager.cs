using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;

public class MapManager : MonoBehaviour
{
    public const int UNITY_LAYER_MAP = 6;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private SkyManager skyManager;
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private MapControl mapControl;
    [SerializeField] private GameObject locationUndefinedWindow;
    private AbstractMap map;
    private bool isMapInitialized;
    private int numberOfTiles;
    private int numberOfTilesInitialized;
    private List<Tile> tiles;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(skyManager);
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(mapControl);
        Assert.IsNotNull(locationUndefinedWindow);

        map = GetComponent<AbstractMap>();
        map.OnTileFinished += TileFinished;

        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        numberOfTiles = (extentOptions.north + 1 + extentOptions.south) * (extentOptions.west + 1 + extentOptions.east);

        numberOfTilesInitialized = 0;
        isMapInitialized = false;
        tiles = new List<Tile>();
    }

    public bool IsMapInitialized()
    {
        return isMapInitialized;
    }

    public void ChangeLocation(Location location)
    {
        tiles.Clear();
        locationUndefinedWindow.SetActive(location == null);
        
        if (location != null) {
            numberOfTilesInitialized = 0;

            map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(location.Coordinate.latitude, location.Coordinate.longitude));
            map.UpdateMap();

            // There is a Mapbox bug with the roads, buildings and elevation, so we reset them 
            bool buildingLayerActive = mapControl.IsBuildingLayerActive();
            DisplayBuildings(!buildingLayerActive);
            DisplayBuildings(buildingLayerActive);

            bool isTerrainElevated = map.Terrain.ElevationType == ElevationLayerType.TerrainWithElevation;
            if (isTerrainElevated) {
                map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
                map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
            } else {
                map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
                map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
            }
        }
    }

    public Vector3 GetUnityPositionFromCoordinates(Coordinate coordinates, bool stickToGround = false)
    {
        Vector3 position = Conversions.GeoToWorldPosition(coordinates.latitude, coordinates.longitude, map.CenterMercator, map.WorldRelativeScale).ToVector3xz();

        if (stickToGround) {
            bool isTerrainElevated = map.Terrain.ElevationType == ElevationLayerType.TerrainWithElevation;
            if (isTerrainElevated) {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(position + new Vector3(0, 10000, 0), Vector3.down, out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                    position.y = hit.point.y; 
                } else {
                    // Fall back to Mapbox elevation if raycast failed
                    position.y = map.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coordinates.latitude, coordinates.longitude));
                }
            } else {
                position.y = 0; 
            }
        } else {
            position.y = (float) coordinates.altitude;
        }
        return position;
    }

    public Coordinate GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return new Coordinate(position.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale), position.y);
    }

    public bool IsCoordinateOnMap(Coordinate coordinates)
    {
        Mapbox.Unity.MeshGeneration.Data.UnityTile tile;
		return map.MapVisualizer.ActiveTiles.TryGetValue(Conversions.LatitudeLongitudeToTileId(coordinates.latitude, coordinates.longitude, (int)map.Zoom), out tile);
    }

    public void SetStyle(int style)
    {
        map.ImageLayer.SetLayerSource((ImagerySourceType) style);
    }

    public void SetElevationType(bool enableElevation)
    {
        if (enableElevation) {
            map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
        } else {
            map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
        }

        foreach (IObjectsManager objectsManager in sceneManager.GetObjectsManagers()) {
            objectsManager.OnLocationChanged();
        }
    }

    public void DisplayBuildings(bool display)
    {
        map.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(display);
    }

    public List<Tile> GetTiles()
    {
        if (tiles.Count == 0) {
            foreach (Transform child in transform) {
                Mapbox.Unity.MeshGeneration.Data.UnityTile tile = child.gameObject.GetComponent<Mapbox.Unity.MeshGeneration.Data.UnityTile>();
                
                if (tile != null) {
                    tiles.Add(new Tile(tile));
                }
            }
        }

        return tiles;
    }

    public (Coordinate, Coordinate) GetTileBoundaries(Tile tile)
    {
        return (GetCoordinatesFromUnityPosition(new Vector3(
            tile.Transform.position.x - tile.MeshFilter.mesh.bounds.size.x / 2,
            tile.Transform.position.y,
            tile.Transform.position.z - tile.MeshFilter.mesh.bounds.size.z / 2
        )), GetCoordinatesFromUnityPosition(new Vector3(
            tile.Transform.position.x + tile.MeshFilter.mesh.bounds.size.x / 2,
            tile.Transform.position.y,
            tile.Transform.position.z + tile.MeshFilter.mesh.bounds.size.z / 2
        )));
    }

    public Vector3 GetMapSize()
    {
        List<Tile> tiles = GetTiles();

        float tileSize = 0;
        if (tiles.Count > 0) {
            tileSize = tiles[0].MeshFilter.mesh.bounds.size.x;
        }
        
        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        return tileSize * new Vector3(1 + extentOptions.west + extentOptions.east, 0, 1 + extentOptions.north + extentOptions.south);
    }

    public string GetGroundFromPosition(Vector3 position)
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(position + new Vector3(0, 10000, 0), Vector3.down, out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
            string ground = groundTexturesManager.GetPositionTexture(hit.transform.gameObject.GetComponent<Renderer>().material, hit.textureCoord);
            if (ground != "") {
                return ground;
            } 
        }

        return "Unknown";
    }

    public void UpdateMapTerrain()
    {
        bool isTerrainElevated = map.Terrain.ElevationType == ElevationLayerType.TerrainWithElevation;
        if (isTerrainElevated) {
            map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
            map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
        } else {
            map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
            map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
        }
    }

    private void TileFinished(Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
    {
        if (!isMapInitialized) {
            isMapInitialized = true;
        }

        numberOfTilesInitialized++;

        if (numberOfTilesInitialized == numberOfTiles) {
            foreach (IObjectsManager objectsManager in sceneManager.GetObjectsManagers()) {
                objectsManager.OnLocationChanged();
            }
            skyManager.OnLocationChanged();
            lightComputationManager.OnLocationChanged();
        }
    }
}