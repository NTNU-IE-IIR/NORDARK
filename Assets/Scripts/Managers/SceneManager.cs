using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using SFB;

public class SceneManager : MonoBehaviour
{
    private static readonly string DEFAULT_SCENE_NAME = "DefaultScene";

    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private TreeManager treeManager;
    [SerializeField]
    private DialogControl dialogControl;
    private string currentSave;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(treeManager);
        Assert.IsNotNull(dialogControl);

        currentSave = "";
    }

    public void LoadDefaultScene()
    {
        GeoJSON.Net.Feature.FeatureCollection featureCollection =
            JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(Resources.Load<TextAsset>(DEFAULT_SCENE_NAME).text);
        LoadScene(featureCollection);
    }

    public void Load()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a NORDARK scene file", "", "geojson", false);
        if (paths.Length > 0) {
            currentSave = paths[0];
            GeoJSON.Net.Feature.FeatureCollection featureCollection = ReadFile(paths[0]);
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
        string filename = StandaloneFileBrowser.SaveFilePanel("Save the current scene", "", "nordark", "geojson");
        if (filename != "") {
            currentSave = filename;
            SaveScene();
        }
    }

    public void AddLights()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Insert lights to the scene", "", "geojson", true);
        foreach (string path in paths) {
            GeoJSON.Net.Feature.FeatureCollection featureCollection = ReadFile(path);

            foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                AddLight(feature);
            }
        }
    }

    private GeoJSON.Net.Feature.FeatureCollection ReadFile(string filename)
    {
        StreamReader rd = new StreamReader(filename);
        GeoJSON.Net.Feature.FeatureCollection featureCollection = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(rd.ReadToEnd());
        rd.Close();
        return featureCollection;
    }

    private void LoadScene(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        lightsManager.ClearLights();
        camerasManager.ClearCameras();
        treeManager.ClearTrees();
        mapManager.ClearLocation();

        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "location")) {
                AddLocation(feature);
            } else if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "camera")) {
                AddCamera(feature);
            } else if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "tree")) {
                AddTree(feature);
            } else if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "light")) {
                AddLight(feature);
            }
        }

        dialogControl.CreateInfoDialog("Scene loaded.");
    }

    private void SaveScene()
    {
        List<Feature> features = lightsManager.GetFeatures();
        features.AddRange(camerasManager.GetFeatures());
        features.AddRange(mapManager.GetFeatures());
        features.AddRange(treeManager.GetFeatures());
        SaveToGeojson(currentSave, features);

        dialogControl.CreateInfoDialog("Scene saved.");
    }

    private void AddLocation(GeoJSON.Net.Feature.Feature feature)
    {
        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;

        Location location = new Location();
        if (feature.Properties.ContainsKey("name")) {
            location.Name = feature.Properties["name"] as string;
        }
        location.Coordinates = new Vector2d(point.Coordinates.Latitude, point.Coordinates.Longitude);
        if (point.Coordinates.Altitude != null) {
            location.Altitude = (double) point.Coordinates.Altitude;
        }
        if (feature.Properties.ContainsKey("cameraCoordinates")) {
            List<double> cameraCoordinates = (feature.Properties["cameraCoordinates"] as Newtonsoft.Json.Linq.JArray).ToObject<List<double>>();
            location.CameraCoordinates = new Vector2d(cameraCoordinates[0], cameraCoordinates[1]);
            location.CameraAltitude = cameraCoordinates[2];
        }
        mapManager.AddLocation(location);
    }

    private void AddCamera(GeoJSON.Net.Feature.Feature feature)
    {
        string name = "";
        if (feature.Properties.ContainsKey("name")) {
            name = feature.Properties["name"] as string;
        }

        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        Vector2d latLong = new Vector2d(point.Coordinates.Latitude, point.Coordinates.Longitude);
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }

        Vector3 eulerAngles = new Vector3(0, 0, 0);
        try
        {
            List<float> eulerAnglesList = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
            eulerAngles = new Vector3(eulerAnglesList[0], eulerAnglesList[1], eulerAnglesList[2]);
        }
        catch (System.Exception)
        {}

        CameraParameters cameraParameters = new CameraParameters();
        try
        {
            cameraParameters = new CameraParameters((feature.Properties["parameters"] as Newtonsoft.Json.Linq.JObject).ToObject<CameraParametersSerialized>());
        }
        catch (System.Exception)
        {}

        camerasManager.CreateCamera(new CameraNode(name, latLong, altitude), eulerAngles, cameraParameters);
    }

    private void AddTree(GeoJSON.Net.Feature.Feature feature)
    {
        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        Vector2d latLong = new Vector2d(point.Coordinates.Latitude, point.Coordinates.Longitude);
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }

        treeManager.CreateTree(new TreeNode(latLong, altitude));
    }

    private void AddLight(GeoJSON.Net.Feature.Feature feature)
    {
        GeoJSON.Net.Geometry.Point point = null;
        if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Point")) {
            point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        } else if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiPoint")) {
            point = (feature.Geometry as GeoJSON.Net.Geometry.MultiPoint).Coordinates[0];
        }

        if (point != null) {
            Vector2d latLong = new Vector2d(point.Coordinates.Latitude, point.Coordinates.Longitude);
            double altitude = 0;
            if (point.Coordinates.Altitude != null) {
                altitude = (double) point.Coordinates.Altitude;
            }
            
            string name = "";
            if (feature.Properties.ContainsKey("name")) {
                name = feature.Properties["name"] as string;
            }
            string prefabName = "";
            if (feature.Properties.ContainsKey("prefabName")) {
                prefabName = feature.Properties["prefabName"] as string;
            }

            Vector3 eulerAngles = new Vector3(0, 0);
            try
            {
                List<float> eulerAnglesList = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
                eulerAngles = new Vector3(eulerAnglesList[0], eulerAnglesList[1], eulerAnglesList[2]);
            }
            catch (System.Exception)
            {}

            string IESName = "";
            if (feature.Properties.ContainsKey("IESfileName")) {
                IESName = feature.Properties["IESfileName"] as string;
            }

            LightNode lightNode = new LightNode(latLong, altitude);
            lightNode.Name = name;
            lightNode.PrefabName = prefabName;
            lightsManager.CreateLight(lightNode, eulerAngles, IESName);
        }
    }

    private void SaveToGeojson(string filename, List<Feature> features)
    {
        GeoJSON.Net.Feature.FeatureCollection featureCollection = new GeoJSON.Net.Feature.FeatureCollection();

        for (int i = 0; i < features.Count; i++) {
            GeoJSON.Net.Geometry.Point point = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(features[i].Coordinates.x, features[i].Coordinates.y, features[i].Coordinates.altitude));

            GeoJSON.Net.Feature.Feature feature = new GeoJSON.Net.Feature.Feature(point, features[i].Properties, i.ToString());
            featureCollection.Features.Add(feature);
        }

        var json = JsonConvert.SerializeObject(featureCollection);

        json = json.Replace("\"type\":8", "\"type\":\"FeatureCollection\"");
        json = json.Replace("\"type\":7", "\"type\":\"Feature\"");
        json = json.Replace("\"type\":5", "\"type\":\"MultiPolygon\"");
        json = json.Replace("\"type\":4", "\"type\":\"Polygon\"");
        json = json.Replace("\"type\":3", "\"type\":\"MultiLineString\"");
        json = json.Replace("\"type\":0", "\"type\":\"Point\"");

        StreamWriter sw = new StreamWriter(filename);
        sw.WriteLine(json);
        sw.Close();
    }
}