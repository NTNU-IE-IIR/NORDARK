using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;

public class MapManager : MonoBehaviour
{
    public const int UNITY_LAYER_MAP = 6;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private VegetationManager vegetationManager;
    [SerializeField] private SkyManager skyManager;
    [SerializeField] private MapControl mapControl;
    [SerializeField] private GameObject locationUndefinedWindow;
    private AbstractMap map;
    private bool isMapInitialized;
    private int numberOfTiles;
    private int numberOfTilesInitialized;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(skyManager);
        Assert.IsNotNull(mapControl);
        Assert.IsNotNull(locationUndefinedWindow);

        map = GetComponent<AbstractMap>();
        map.OnTileFinished += TileFinished;

        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        numberOfTiles = 1 + 2*extentOptions.north + 2*extentOptions.west + 2*extentOptions.south + 2*extentOptions.east;

        numberOfTilesInitialized = 0;
        isMapInitialized = false;
    }

    public bool IsMapInitialized()
    {
        return isMapInitialized;
    }

    public void ChangeLocation(Location location)
    {
        locationUndefinedWindow.SetActive(location == null);
        
        if (location != null) {
            numberOfTilesInitialized = 0;

            map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(location.Coordinates.x, location.Coordinates.y));
            map.UpdateMap();

            // There is a Mapbox bug with the roads, buildings and elevation, so we reset them 
            bool buildingLayerActive = mapControl.IsBuildingLayerActive();
            DisplayBuildings(!buildingLayerActive);
            DisplayBuildings(buildingLayerActive);

            bool roadLayerActive = mapControl.IsRoadLayerActive();
            DisplayRoads(!roadLayerActive);
            DisplayRoads(roadLayerActive);

            bool isTerrainElevated = map.Terrain.ElevationType == ElevationLayerType.TerrainWithElevation;
            if (isTerrainElevated) {
                map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
                map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
            } else {
                map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
                map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
            }

            foreach (IObjectsManager objectsManager in sceneManager.GetObjectsManagers()) {
                objectsManager.OnLocationChanged();
            }
            skyManager.OnLocationChanged();
        }
    }

    public Vector3 GetUnityPositionFromCoordinates(Vector3d coordinates, bool stickToGround = false)
    {
        Vector3 position = (new Vector2((float) coordinates.x, (float) coordinates.y)).AsUnityPosition(map.CenterMercator, map.WorldRelativeScale);
        if (stickToGround) {
            position.y = map.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coordinates.x, coordinates.y));
        } else {
            position.y = (float) coordinates.altitude;
        }
        return position;
    }

    public Vector3d GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return new Vector3d(position.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale), position.y);
    }

    public bool IsCoordinateOnMap(Vector2d latlong)
    {
        Mapbox.Unity.MeshGeneration.Data.UnityTile tile;
		return map.MapVisualizer.ActiveTiles.TryGetValue(Conversions.LatitudeLongitudeToTileId(latlong.x, latlong.y, (int)map.Zoom), out tile);
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

    public float GetWorldRelativeScale()
    {
        return map.WorldRelativeScale;
    }

    public void DisplayBuildings(bool display)
    {
        map.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(display);
    }

    public void DisplayRoads(bool display)
    {
        map.VectorData.GetFeatureSubLayerAtIndex(1).SetActive(display);
    }

    public List<MeshRenderer> GetTilesMeshRenderer()
    {
        List<MeshRenderer> tiles = new List<MeshRenderer>();
        foreach (Transform child in transform) {
            MeshRenderer meshRenderer = child.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null) {
                tiles.Add(meshRenderer);
            }
        }
        return tiles;
    }

    public List<Mapbox.Unity.MeshGeneration.Data.UnityTile> GetTiles()
    {
        List<Mapbox.Unity.MeshGeneration.Data.UnityTile> tiles = new List<Mapbox.Unity.MeshGeneration.Data.UnityTile>();
        foreach (Transform child in transform) {
            Mapbox.Unity.MeshGeneration.Data.UnityTile tile = child.gameObject.GetComponent<Mapbox.Unity.MeshGeneration.Data.UnityTile>();
            if (tile != null) {
                tiles.Add(tile);
            }
        }
        return tiles;
    }

    private void TileFinished(Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
    {
        if (!isMapInitialized) {
            isMapInitialized = true;
        }

        if (tile.MeshRenderer.gameObject.transform.parent == transform) {
            numberOfTilesInitialized++;

            if (numberOfTilesInitialized == numberOfTiles) {
                vegetationManager.GenerateBiomes();
            }
        }
    }
}