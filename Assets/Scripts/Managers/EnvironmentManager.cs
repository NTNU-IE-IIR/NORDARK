using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnvironmentManager : MonoBehaviour
{
    private enum Environment
    {
        Map,
        Scene3D
    }

    [SerializeField]
    private GameObject map;
    [SerializeField]
    private GameObject scene3D;
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private Scene3DManager scene3DManager;
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private TreeManager treeManager;
    [SerializeField]
    private SiteControl siteControl;

    private List<Location> locations;

    void Awake()
    {
        Assert.IsNotNull(map);
        Assert.IsNotNull(scene3D);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(scene3DManager);
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(treeManager);
        Assert.IsNotNull(siteControl);

        locations = new List<Location>();
    }

    public void SetDefaultEnvironment()
    {
        ShowMap();
    }

    public bool isEnvironmentReady()
    {
        return mapManager.IsMapInitialized();
    }

    public void ShowMap()
    {
        ShowEnvironment(Environment.Map);
    }

    public void Show3DScene()
    {
        ShowEnvironment(Environment.Scene3D);
    }

    public Vector3 GetUnityPositionFromCoordinatesAndAltitude(Vector2d latLong, double altitude, bool stickToGround = false)
    {
        if (map.activeSelf) {
            return mapManager.GetUnityPositionFromCoordinatesAndAltitude(latLong, altitude, stickToGround);
        } else {
            return scene3DManager.GetUnityPositionFromCoordinatesAndAltitude(latLong, altitude, stickToGround);
        }
    }

    public float GetWorldRelativeScale()
    {
        if (map.activeSelf) {
            return mapManager.GetWorldRelativeScale();
        } else {
            return scene3DManager.GetWorldRelativeScale();
        }
    }

    public Vector2d GetCoordinatesFromUnityPosition(Vector3 position)
    {
        if (map.activeSelf) {
            return mapManager.GetCoordinatesFromUnityPosition(position);
        } else {
            return scene3DManager.GetCoordinatesFromUnityPosition(position);
        }
    }

    public double GetAltitudeFromUnityPosition(Vector3 position)
    {
        if (map.activeSelf) {
            return mapManager.GetAltitudeFromUnityPosition(position);
        } else {
            return scene3DManager.GetAltitudeFromUnityPosition(position);
        }
    }

    public void AddLocation(Location location)
    {
        locations.Add(location);
        siteControl.AddLocation(location.Name);

        ChangeLocation(locations.Count - 1);
    }

    public void ChangeLocation(int locationIndex)
    {
        mapManager.SetLocation(locations[locationIndex].Coordinates);
        scene3DManager.SetLocation(locations[locationIndex]);
        camerasManager.SetMainCameraPosition(locations[locationIndex].CameraCoordinates, locations[locationIndex].CameraAltitude);
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
            feature.Properties.Add("unityUnitsPerLongitude", location.UnityUnitsPerLongitude);
            feature.Properties.Add("unityUnitsPerLatitude", location.UnityUnitsPerLatitude);
            feature.Properties.Add("unityUnitsPerMeters", location.UnityUnitsPerMeters);
            feature.Properties.Add("worldRelativeScale", location.WorldRelativeScale);
            feature.Properties.Add("cameraCoordinates", new List<double>{location.CameraCoordinates.x, location.CameraCoordinates.y, location.CameraAltitude});
            feature.Coordinates = new Vector3d(location.Coordinates, location.Altitude);
            features.Add(feature);
        }
        return features;
    }

    static public int GetEnvironmentLayer()
    {
        return (1 << MapManager.UNITY_LAYER_MAP) | (1 << Scene3DManager.UNITY_LAYER_TERRAIN);
    }

    private void ShowEnvironment(Environment environment)
    {
        map.SetActive(false);
        scene3D.SetActive(false);

        if (environment == Environment.Map) {
            map.SetActive(true);
            treeManager.Show(true);
        } else {
            scene3D.SetActive(true);
            treeManager.Show(false);
        }

        lightsManager.UpdateLightsPositions();
        camerasManager.UpdateCamerasPosition();
    }
}