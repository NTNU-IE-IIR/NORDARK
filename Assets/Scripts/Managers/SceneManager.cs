using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

public class SceneManager : MonoBehaviour
{
    private static readonly string DEFAULT_SCENE_NAME = "DefaultScene";
    [SerializeField] private LightsManager lightsManager;
    [SerializeField] private CamerasManager camerasManager;
    [SerializeField] private VegetationManager vegetationManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private DataVisualizationManager dataVisualizationManager;
    [SerializeField] private DialogControl dialogControl;
    private string currentSave;
    private List<IObjectsManager> objectsManagers;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(dataVisualizationManager);
        Assert.IsNotNull(dialogControl);

        currentSave = "";
        objectsManagers = new List<IObjectsManager>{
            lightsManager, camerasManager, vegetationManager, locationsManager, groundTexturesManager, dataVisualizationManager
        };
    }

    public void LoadDefaultScene()
    {
        GeoJSON.Net.Feature.FeatureCollection featureCollection =
            JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(Resources.Load<TextAsset>(DEFAULT_SCENE_NAME).text);
        LoadScene(featureCollection);
    }

    public void Load()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select a NORDARK scene file", "", "geojson", false);
        if (paths.Length > 0) {
            currentSave = paths[0];
            GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(paths[0]);
            LoadScene(featureCollection);
        }
    }

    public void Save()
    {
        if (currentSave == "") {
            SaveAs();
        } else {
            SaveScene();
        }
    }

    public void SaveAs()
    {
        string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Save the current scene", "", "nordark", "geojson");
        if (filename != "") {
            currentSave = filename;
            SaveScene();
        }
    }

    public List<IObjectsManager> GetObjectsManagers()
    {
        return objectsManagers;
    }

    private void LoadScene(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        foreach(IObjectsManager objectsManager in objectsManagers) {
            objectsManager.Clear();
        }

        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("type")) {
                if (string.Equals(feature.Properties["type"] as string, "location")) {
                    locationsManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "camera")) {
                    camerasManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "light")) {
                    lightsManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "biomeArea")) {
                    vegetationManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "groundTexture")) {
                    groundTexturesManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "dataset")) {
                    dataVisualizationManager.Create(feature);
                }
            }
        }
        dialogControl.CreateInfoDialog("Scene loaded.");
    }

    private void SaveScene()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();
        foreach(IObjectsManager objectsManager in objectsManagers) {
            features.AddRange(objectsManager.GetFeatures());
        }
        
        GeoJSONParser.FeaturesToFile(currentSave, features);
        dialogControl.CreateInfoDialog("Scene saved.");
    }
}