using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectsManager : MonoBehaviour
{
    protected bool changesUnsaved;
    protected abstract void CreateObject(GeoJSON.Net.Feature.Feature feature, Location location);
    protected abstract void ClearActiveObjects();
    protected abstract void OnAfterLocationChanged();
    protected abstract List<GeoJSON.Net.Feature.Feature> GetFeaturesOfCurrentLocation();

    protected List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();
    [SerializeField] protected LocationsManager locationsManager;

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        features.Add(feature);
        CreateObjectIfInCurrentLocation(feature);
    }

    public void Clear()
    {
        features.Clear();
        ClearActiveObjects();
    }

    public void OnLocationChanged()
    {
        ClearActiveObjects();

        foreach (GeoJSON.Net.Feature.Feature feature in features) {
            CreateObjectIfInCurrentLocation(feature);
        }

        changesUnsaved = false;

        OnAfterLocationChanged();
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> allFeatures = GetFeaturesOfCurrentLocation();

        Location currentLocation = locationsManager.GetCurrentLocation();
        foreach (GeoJSON.Net.Feature.Feature feature in features) {
            string location = "";
            if (feature.Properties.ContainsKey("location")) {
                location = feature.Properties["location"] as string;
            }
            if (currentLocation != null && location != currentLocation.Name) {
                allFeatures.Add(feature);
            }
        }

        features = allFeatures;
        return features;
    }

    public bool AreThereChangesUnsaved()
    {
        return changesUnsaved;
    }

    public void SetChangesSaved()
    {
        changesUnsaved = false;
    }

    private void CreateObjectIfInCurrentLocation(GeoJSON.Net.Feature.Feature feature)
    {
        string location = "";
        if (feature.Properties.ContainsKey("location")) {
            location = feature.Properties["location"] as string;
        }
        Location currentLocation = locationsManager.GetCurrentLocation();

        if (currentLocation != null && currentLocation.Name == location) {
            CreateObject(feature, currentLocation);
        }
    }
}
