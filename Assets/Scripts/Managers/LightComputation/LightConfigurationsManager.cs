using UnityEngine;
using UnityEngine.Assertions;

public class LightConfigurationsManager : MonoBehaviour
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private ConfigurationsControl configurationsControl;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(configurationsControl);
    }

    public void ResetConfigurations()
    {
        // Configuration 0 is the main configuration, it shouldn't be deleted
        for (int i=1; i<ConfigurationsControl.MAX_NUMBER_OF_CONFIGURATIONS; ++i) {
            lightPolesManager.DeleteAllLightPolesFromConfiguration(i);
        }
    }

    public void SetConfiguration(string path, int configurationIndex)
    {
        lightPolesManager.DeleteAllLightPolesFromConfiguration(configurationIndex);

        GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(path);
        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "light")) {
                lightPolesManager.Create(feature, configurationIndex);
            }
        }
    }

    public int GetCurrentNumberOfConfigurations()
    {
        return configurationsControl.GetCurrentNumberOfConfigurations();
    }
}
