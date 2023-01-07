using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LocationsManager : MonoBehaviour, IObjectsManager
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private LocationControl locationControl;
    private List<Location> locations;
    private int currentLocationIndex;

    void Awake()
    {
        Assert.IsNotNull(locationControl);

        locations = new List<Location>();
        currentLocationIndex = -1;
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        Location location = new Location();

        if (feature.Properties.ContainsKey("name")) {
            location.Name = feature.Properties["name"] as string;
        }
        
        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }
        location.Coordinates = new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude);

        List<double> cameraCoordinates = new List<double>();
        if (feature.Properties.ContainsKey("cameraCoordinates")) {
            cameraCoordinates = (feature.Properties["cameraCoordinates"] as Newtonsoft.Json.Linq.JArray).ToObject<List<double>>();
        }
        if (cameraCoordinates.Count < 3) {
            cameraCoordinates = new List<double>{ 0, 0, 0 };
        }
        location.CameraCoordinates = new Vector3d(cameraCoordinates[0], cameraCoordinates[1], cameraCoordinates[2]);

        List<float> cameraAngles = new List<float>();
        if (feature.Properties.ContainsKey("cameraAngles")) {
            cameraAngles = (feature.Properties["cameraAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
        }
        if (cameraAngles.Count < 3) {
            cameraAngles = new List<float>{ 0, 0, 0 };
        }
        location.CameraAngles = new Vector3(cameraAngles[0], cameraAngles[1], cameraAngles[2]);

        AddLocation(location);
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
                location.Coordinates.x,
                location.Coordinates.y,
                location.Coordinates.altitude
            ));
            
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("type", "location");
            properties.Add("cameraCoordinates", new List<double>{location.CameraCoordinates.x, location.CameraCoordinates.y, location.CameraCoordinates.altitude});
            properties.Add("cameraAngles", new List<float>{location.CameraAngles.x, location.CameraAngles.y, location.CameraAngles.z});

            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
        }

        return features;
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

    public void ChangeLocation(int locationIndex)
    {
        currentLocationIndex = locationIndex;
        mapManager.ChangeLocation(GetCurrentLocation());
    }

    public void DeleteCurrentLocation(int locationIndex)
    {
        locations.RemoveAt(locationIndex);

        if (locations.Count >= 0) {
            ChangeLocation(0);
        } else {
            ChangeLocation(-1);
        }
    }
}
