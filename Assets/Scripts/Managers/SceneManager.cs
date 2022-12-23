using System.Collections.Generic;
using System.IO;
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
        Assert.IsNotNull(dialogControl);

        currentSave = "";
        objectsManagers = new List<IObjectsManager>{lightsManager, camerasManager, vegetationManager, locationsManager, groundTexturesManager};
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
            GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONReader.ReadFile(paths[0]);
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

    public void AddLights()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Insert lights to the scene", "", "geojson", true);
        foreach (string path in paths) {
            GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONReader.ReadFile(path);

            foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                AddLight(feature);
            }
        }
    }

    public void AddGroundTextures()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Insert ground textures to the scene", "", "geojson", true);
        foreach (string path in paths) {
            GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONReader.ReadFile(path);

            foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                AddGroundTexture(feature);
            }
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
                    AddLocation(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "camera")) {
                    AddCamera(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "light")) {
                    AddLight(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "biomeArea")) {
                    AddBiomeArea(feature);
                } else if (string.Equals(feature.Properties["type"] as string, "groundTexture")) {
                    AddGroundTexture(feature);
                }
            }
        }
        dialogControl.CreateInfoDialog("Scene loaded.");
    }

    private void SaveScene()
    {
        List<Feature> features = new List<Feature>();
        foreach(IObjectsManager objectsManager in objectsManagers) {
            features.AddRange(objectsManager.GetFeatures());
        }
        
        GeoJSONReader.SaveToGeojson(currentSave, features);
        dialogControl.CreateInfoDialog("Scene saved.");
    }

    private void AddLocation(GeoJSON.Net.Feature.Feature geoJsonFeature)
    {
        GeoJSON.Net.Geometry.Point point = geoJsonFeature.Geometry as GeoJSON.Net.Geometry.Point;

        string name = "";
        if (geoJsonFeature.Properties.ContainsKey("name")) {
            name = geoJsonFeature.Properties["name"] as string;
        }

        List<double> cameraCoordinates = new List<double>();
        if (geoJsonFeature.Properties.ContainsKey("cameraCoordinates")) {
            cameraCoordinates = (geoJsonFeature.Properties["cameraCoordinates"] as Newtonsoft.Json.Linq.JArray).ToObject<List<double>>();
        }
        if (cameraCoordinates.Count < 3) {
            cameraCoordinates = new List<double>{ 0, 0, 0 };
        }

        List<float> cameraAngles = new List<float>();
        if (geoJsonFeature.Properties.ContainsKey("cameraAngles")) {
            cameraAngles = (geoJsonFeature.Properties["cameraAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
        }
        if (cameraAngles.Count < 3) {
            cameraAngles = new List<float>{ 0, 0, 0 };
        }

        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }

        Feature feature = new Feature();
        feature.Properties.Add("type", "location");
        feature.Properties.Add("name", name);
        feature.Properties.Add("cameraCoordinates", cameraCoordinates);
        feature.Properties.Add("cameraAngles", cameraAngles);
        feature.Coordinates = new List<Vector3d> {new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude)};
        locationsManager.Create(feature);
    }

    private void AddCamera(GeoJSON.Net.Feature.Feature geoJsonFeature)
    {
        string name = "";
        if (geoJsonFeature.Properties.ContainsKey("name")) {
            name = geoJsonFeature.Properties["name"] as string;
        }

        List<float> eulerAngles = new List<float>();
        try
        {
            eulerAngles = (geoJsonFeature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
        }
        catch (System.Exception)
        {}
        if (eulerAngles.Count < 3) {
            eulerAngles = new List<float>{ 0, 0, 0 };
        }

        CameraParameters cameraParameters = new CameraParameters();
        try
        {
            cameraParameters = new CameraParameters((geoJsonFeature.Properties["parameters"] as Newtonsoft.Json.Linq.JObject).ToObject<CameraParametersSerialized>());
        }
        catch (System.Exception)
        {}

        GeoJSON.Net.Geometry.Point point = geoJsonFeature.Geometry as GeoJSON.Net.Geometry.Point;
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }

        Feature feature = new Feature();
        feature.Properties.Add("type", "camera");
        feature.Properties.Add("name", name);
        feature.Properties.Add("eulerAngles", eulerAngles);
        feature.Properties.Add("parameters", cameraParameters);
        feature.Coordinates = new List<Vector3d> {new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude)};
        camerasManager.Create(feature);
    }

    private void AddLight(GeoJSON.Net.Feature.Feature geoJsonFeature)
    {
        GeoJSON.Net.Geometry.Point point = null;
        if (string.Equals(geoJsonFeature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Point")) {
            point = geoJsonFeature.Geometry as GeoJSON.Net.Geometry.Point;
        } else if (string.Equals(geoJsonFeature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiPoint")) {
            point = (geoJsonFeature.Geometry as GeoJSON.Net.Geometry.MultiPoint).Coordinates[0];
        }

        if (point != null) {
            string name = "";
            if (geoJsonFeature.Properties.ContainsKey("name")) {
                name = geoJsonFeature.Properties["name"] as string;
            }

            List<float> eulerAngles = new List<float>();
            try
            {
                eulerAngles = (geoJsonFeature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
            }
            catch (System.Exception)
            {}
            if (eulerAngles.Count < 3) {
                eulerAngles = new List<float>{ 0, 0, 0 };
            }

            string IESName = "";
            if (geoJsonFeature.Properties.ContainsKey("IESfileName")) {
                IESName = geoJsonFeature.Properties["IESfileName"] as string;
            }

            string prefabName = "";
            if (geoJsonFeature.Properties.ContainsKey("prefabName")) {
                prefabName = geoJsonFeature.Properties["prefabName"] as string;
            }

            double altitude = 0;
            if (point.Coordinates.Altitude != null) {
                altitude = (double) point.Coordinates.Altitude;
            }

            Feature feature = new Feature();
            feature.Properties.Add("type", "light");
            feature.Properties.Add("name", name);
            feature.Properties.Add("eulerAngles", eulerAngles);
            feature.Properties.Add("IESfileName", IESName);
            feature.Properties.Add("prefabName", prefabName);
            feature.Coordinates = new List<Vector3d> {new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude)};
            lightsManager.Create(feature);
        }
    }

    private void AddBiomeArea(GeoJSON.Net.Feature.Feature geoJsonFeature)
    {
        if (string.Equals(geoJsonFeature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Polygon")) {
            string name = "";
            if (geoJsonFeature.Properties.ContainsKey("name")) {
                name = geoJsonFeature.Properties["name"] as string;
            }

            string biome = "";
            if (geoJsonFeature.Properties.ContainsKey("biome")) {
                biome = geoJsonFeature.Properties["biome"] as string;
            }

            GeoJSON.Net.Geometry.Polygon polygon = geoJsonFeature.Geometry as GeoJSON.Net.Geometry.Polygon;

            Feature feature = new Feature();
            feature.Properties.Add("type", "biomeArea");
            feature.Properties.Add("name", name);
            feature.Properties.Add("biome", biome);
            foreach (GeoJSON.Net.Geometry.Position coordinate in polygon.Coordinates[0].Coordinates) {
                feature.Coordinates.Add(new Vector3d(coordinate.Latitude, coordinate.Longitude, 0));
            }
            vegetationManager.Create(feature);
        }
    }

    private void AddGroundTexture(GeoJSON.Net.Feature.Feature geoJsonFeature)
    {
        if (string.Equals(geoJsonFeature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Polygon")) {
            int texture = 1;
            if (geoJsonFeature.Properties.ContainsKey("texture")) {
                texture = System.Convert.ToInt32(geoJsonFeature.Properties["texture"]);
            }

            GeoJSON.Net.Geometry.Polygon polygon = geoJsonFeature.Geometry as GeoJSON.Net.Geometry.Polygon;

            Feature feature = new Feature();
            feature.Properties.Add("type", "biomeArea");
            feature.Properties.Add("texture", texture);
            foreach (GeoJSON.Net.Geometry.Position coordinate in polygon.Coordinates[0].Coordinates) {
                feature.Coordinates.Add(new Vector3d(coordinate.Latitude, coordinate.Longitude, 0));
            }
            groundTexturesManager.Create(feature);
        }
    }
}