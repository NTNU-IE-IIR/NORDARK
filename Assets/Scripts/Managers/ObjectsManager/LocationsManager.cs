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

    public void Create(Feature feature)
    {
        Location location = new Location();
        location.Name = feature.Properties["name"] as string;
        location.Coordinates = feature.Coordinates[0];
        List<double> cameraCoordinates = feature.Properties["cameraCoordinates"] as List<double>;
        location.CameraCoordinates = new Vector3d(cameraCoordinates[0], cameraCoordinates[1], cameraCoordinates[2]);
        List<float> cameraAngles = feature.Properties["cameraAngles"] as List<float>;
        location.CameraAngles = new Vector3(cameraAngles[0], cameraAngles[1], cameraAngles[2]);

        locations.Add(location);
        locationControl.AddLocation(location.Name);

        ChangeLocation(locations.Count - 1);
    }

    public void Clear()
    {
        currentLocationIndex = -1;
        locations.Clear();
        locationControl.ClearLocations();
    }

    public void OnLocationChanged()
    {
        locationControl.ChangeLocation(currentLocationIndex);
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (Location location in locations) {
            Feature feature = new Feature();
            feature.Properties.Add("name", location.Name);
            feature.Properties.Add("type", "location");
            feature.Properties.Add("cameraCoordinates", new List<double>{location.CameraCoordinates.x, location.CameraCoordinates.y, location.CameraCoordinates.altitude});
            feature.Properties.Add("cameraAngles", new List<float>{location.CameraAngles.x, location.CameraAngles.y, location.CameraAngles.z});
            feature.Coordinates = new List<Vector3d> {location.Coordinates};
            features.Add(feature);
        }
        return features;
    }

    public Location GetCurrentLocation()
    {
        return locations[currentLocationIndex];
    }

    public void ChangeLocation(int locationIndex)
    {
        currentLocationIndex = locationIndex;
        mapManager.ChangeLocation(GetCurrentLocation());
    }
}
