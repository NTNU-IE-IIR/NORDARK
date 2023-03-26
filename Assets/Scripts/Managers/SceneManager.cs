using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

public class SceneManager : MonoBehaviour
{
    private static readonly string DEFAULT_SCENE_NAME = "DefaultScene";
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private CamerasManager camerasManager;
    [SerializeField] private BiomeAreasManager biomeAreasManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private DataVisualizationManager dataVisualizationManager;
    [SerializeField] private VegetationObjectsManager vegetationObjectsManager;
    private string currentSave;
    private List<ObjectsManager> objectsManagers;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(biomeAreasManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(dataVisualizationManager);
        Assert.IsNotNull(vegetationObjectsManager);

        currentSave = "";
        objectsManagers = new List<ObjectsManager>{
            lightPolesManager, camerasManager, biomeAreasManager, groundTexturesManager, dataVisualizationManager, vegetationObjectsManager
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
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select a NORDARK scene file", "", "nordark", false);
        if (paths.Length > 0) {
            try {
                GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(paths[0]);
                currentSave = paths[0];
                LoadScene(featureCollection);
            } catch (System.Exception e) {
                DialogControl.CreateDialog(e.Message);
            }
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
        string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Save the current scene", "", "nordark", "nordark");
        if (filename != "") {
            currentSave = filename;
            SaveScene();
        }
    }

    public void SendOnLocationChangedToAllObjects()
    {
        foreach (ObjectsManager objectsManager in objectsManagers) {
            objectsManager.OnLocationChanged();
        }
    }

    public bool AreThereChangesUnsaved()
    {
        foreach(ObjectsManager objectsManager in objectsManagers) {
            if (objectsManager.AreThereChangesUnsaved()) {
                return true;
            }
        }
        return false;
    }

    private void LoadScene(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        foreach(ObjectsManager objectsManager in objectsManagers) {
            objectsManager.Clear();
        }
        locationsManager.Clear();

        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("type")) {
                if (string.Equals(feature.Properties["type"] as string, "location")) {
                    locationsManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "camera")) {
                    camerasManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "light")) {
                    lightPolesManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "biomeArea")) {
                    biomeAreasManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "groundTexture")) {
                    groundTexturesManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "dataset")) {
                    dataVisualizationManager.Create(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "vegetationObject")) {
                    vegetationObjectsManager.Create(feature);
                }
            }
        }
        locationsManager.ChangeLocation(0);
    }

    private void SaveScene()
    {
        foreach(ObjectsManager objectsManager in objectsManagers) {
            objectsManager.SetChangesSaved();
        }

        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();
        foreach(ObjectsManager objectsManager in objectsManagers) {
            features.AddRange(objectsManager.GetFeatures());
        }
        features.AddRange(locationsManager.GetFeatures());
        
        GeoJSONParser.FeaturesToFile(currentSave, features);
    }
}