using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LocationsManager: MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private LocationControl locationControl;
    [SerializeField] private GameObject locationUndefinedWindow;
    private List<Location> locations;
    private int currentLocationIndex;

    void Awake()
    {
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(locationControl);
        Assert.IsNotNull(locationUndefinedWindow);

        locations = new List<Location>();
        currentLocationIndex = -1;
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        Location location = new Location();

        if (feature.Properties.ContainsKey("name")) {
            location.Name = feature.Properties["name"] as string;
        }
        
        if (feature.Properties.ContainsKey("locationType")) {
            location.Type = (Location.TerrainType) System.Convert.ToInt32(feature.Properties["locationType"]);
        }
        
        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }
        location.Coordinate = new Coordinate(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude);

        if (feature.Properties.ContainsKey("zoom")) {
            location.Zoom = System.Convert.ToInt32(feature.Properties["zoom"]);
        }

        List<double> cameraCoordinates = new List<double>();
        if (feature.Properties.ContainsKey("cameraCoordinates")) {
            cameraCoordinates = (feature.Properties["cameraCoordinates"] as Newtonsoft.Json.Linq.JArray).ToObject<List<double>>();
        }
        if (cameraCoordinates.Count < 3) {
            cameraCoordinates = new List<double>{ 0, 0, 0 };
        }
        location.CameraCoordinates = new Coordinate(cameraCoordinates[0], cameraCoordinates[1], cameraCoordinates[2]);

        List<float> cameraAngles = new List<float>();
        if (feature.Properties.ContainsKey("cameraAngles")) {
            cameraAngles = (feature.Properties["cameraAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
        }
        if (cameraAngles.Count < 3) {
            cameraAngles = new List<float>{ 0, 0, 0 };
        }
        location.CameraAngles = new Vector3(cameraAngles[0], cameraAngles[1], cameraAngles[2]);

        locations.Add(location);
        locationControl.AddLocation(location.Name);
    }

    public void Clear()
    {
        locations.Clear();
        locationControl.ClearLocations();
        ChangeLocation(-1);
    }

    public void OnLocationChanged()
    {
        locationControl.ChangeLocation(currentLocationIndex);
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (Location location in locations) {
            GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                location.Coordinate.latitude,
                location.Coordinate.longitude,
                location.Coordinate.altitude
            ));
            
            Dictionary<string, object> properties = new Dictionary<string, object>() {
                {"type", "location"},
                {"name", location.Name},
                {"locationType", location.Type},
                {"zoom", location.Zoom},
                {"cameraCoordinates", new List<double>{location.CameraCoordinates.latitude, location.CameraCoordinates.longitude, location.CameraCoordinates.altitude}},
                {"cameraAngles", new List<float>{location.CameraAngles.x, location.CameraAngles.y, location.CameraAngles.z}},
            };
            
            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
        }

        return features;
    }

    public bool DoesLocationNameAlreadyExists(string locationName)
    {
        return locations.Select(location => location.Name).Contains(locationName);
    }

    public void AddLocation(Location location)
    {
        locations.Add(location);
        locationControl.AddLocation(location.Name);

        ChangeLocation(locations.Count - 1);
    }

    public Location GetCurrentLocation()
    {
        try {
            return locations[currentLocationIndex];
        } catch (System.ArgumentOutOfRangeException) {
            return null;
        }
    }

    public float GetCurrentTerrainMultiplier()
    {
        // See https://docs.mapbox.com/help/glossary/zoom-level/ for an explanation of the formula
        Location currentLocation = GetCurrentLocation();
        float zoom = currentLocation == null ? MapManager.DEFAULT_ZOOM : currentLocation.Zoom;
        return Mathf.Pow(2, MapManager.DEFAULT_ZOOM - zoom);
    }

    public void ChangeLocation(int locationIndex)
    {
        locationUndefinedWindow.SetActive(locationIndex == -1);
        currentLocationIndex = locationIndex;
        terrainManager.ChangeLocation(GetCurrentLocation());
    }

    public void DeleteCurrentLocation(int locationIndex)
    {
        locations.RemoveAt(locationIndex);

        if (locations.Count > 0) {
            ChangeLocation(0);
        } else {
            ChangeLocation(-1);
        }
    }
}
