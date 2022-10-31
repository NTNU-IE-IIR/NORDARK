using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;

public class MapManager : MonoBehaviour
{
    public const int UNITY_LAYER_MAP = 6;

    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private VegetationManager vegetationManager;
    [SerializeField]
    private SiteControl siteControl;

    private AbstractMap map;
    private bool isMapInitialized;
    private List<Location> locations;
    private int numberOfTiles;
    private int numberOfTilesInitialized;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(siteControl);

        map = GetComponent<AbstractMap>();
        map.Terrain.AddToUnityLayer(UNITY_LAYER_MAP);
        map.OnTileFinished += TileFinished;

        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        numberOfTiles = 1 + 2*extentOptions.north + 2*extentOptions.west + 2*extentOptions.south + 2*extentOptions.east;

        numberOfTilesInitialized = 0;
        isMapInitialized = false;
        locations = new List<Location>();
    }

    public bool IsMapInitialized()
    {
        return isMapInitialized;
    }

    public void AddLocation(Location location)
    {
        locations.Add(location);
        siteControl.AddLocation(location.Name);

        ChangeLocation(locations.Count - 1);
    }

    public void ChangeLocation(int locationIndex)
    {
        numberOfTilesInitialized = 0;

        map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(locations[locationIndex].Coordinates.x, locations[locationIndex].Coordinates.y));
        map.UpdateMap();

        // There is a Mapbox bug with the roads, buildings and elevation, so we reset them 
        bool buildingLayerActive = siteControl.IsBuildingLayerActive();
        DisplayBuildings(!buildingLayerActive);
        DisplayBuildings(buildingLayerActive);

        bool roadLayerActive = siteControl.IsRoadLayerActive();
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
        
        lightsManager.UpdateLightsPositions();
        camerasManager.UpdateCamerasPosition();
        camerasManager.SetMainCameraPosition(locations[locationIndex].CameraCoordinates, locations[locationIndex].CameraAltitude);
        camerasManager.SetMainCameraAngles(locations[locationIndex].CameraAngles);
        siteControl.ChangeLocation(locationIndex);
    }

    public void ClearLocation()
    {
        locations.Clear();
        siteControl.ClearLocations();
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (Location location in locations) {
            Feature feature = new Feature();
            feature.Properties.Add("name", location.Name);
            feature.Properties.Add("type", "location");
            feature.Properties.Add("cameraCoordinates", new List<double>{location.CameraCoordinates.x, location.CameraCoordinates.y, location.CameraAltitude});
            feature.Properties.Add("cameraAngles", new List<float>{location.CameraAngles.x, location.CameraAngles.y, location.CameraAngles.z});
            feature.Coordinates = new List<Vector3d> {new Vector3d(location.Coordinates, location.Altitude)};
            features.Add(feature);
        }
        return features;
    }

    public Vector3 GetUnityPositionFromCoordinatesAndAltitude(Vector2d latLong, double altitude, bool stickToGround = false)
    {
        Vector3 position = (new Vector2((float) latLong.x, (float) latLong.y)).AsUnityPosition(map.CenterMercator, map.WorldRelativeScale);
        if (stickToGround) {
            position.y = GetElevationInUnityUnitsFromCoordinates(latLong);
        } else {
            position.y = (float) altitude;
        }
        return position;
    }

    public float GetElevationInUnityUnitsFromCoordinates(Vector2d latLong)
    {
        return map.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(latLong.x, latLong.y));
    }

    public Vector2d GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return new Vector2d(position.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale));
    }

    public float GetAltitudeFromUnityPosition(Vector3 position)
    {
        return position.y;
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
        lightsManager.UpdateLightsPositions();
        camerasManager.UpdateCamerasPosition();
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

    public List<MeshRenderer> GetTiles()
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

    public Vector2d GetCurrentLocationCoordinates()
    {
        return new Vector2d(map.CenterLatitudeLongitude);
    }

    private float GetElevationFromCoordinates(Vector2d latLong)
    {
        return map.QueryElevationInMetersAt(new Mapbox.Utils.Vector2d(latLong.x, latLong.y));
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