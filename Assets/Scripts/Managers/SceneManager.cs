using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
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
    private EnvironmentManager environmentManager;
    [SerializeField]
    private MainCameraManager mainCameraManager;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(environmentManager);
        Assert.IsNotNull(mainCameraManager);
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
        if (paths.Length > 0)
        {
            GeoJSON.Net.Feature.FeatureCollection featureCollection = ReadFile(paths[0]);
            LoadScene(featureCollection);
        }
    }

    public void Save()
    {
        List<Feature> features = lightsManager.GetFeatures();
        features.AddRange(camerasManager.GetFeatures());
        features.AddRange(environmentManager.GetFeatures());

        string filename = StandaloneFileBrowser.SaveFilePanel("Save the current scene", "", "nordark", "geojson");
        if (filename != "")
        {
            SaveToGeojson(filename, features);
        }
    }

    private void LoadScene(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        lightsManager.ClearLights();
        camerasManager.ClearCameras();

        bool atLeastOneCameraCreated = false;

        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "location")) {
                GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;

                Location location = new Location();
                if (feature.Properties.ContainsKey("name")) {
                    location.Name = feature.Properties["name"] as string;
                }
                location.Coordinates = new Vector2d(point.Coordinates.Latitude, point.Coordinates.Longitude);
                if (point.Coordinates.Altitude != null) {
                    location.Altitude = (double) point.Coordinates.Altitude;
                }
                if (feature.Properties.ContainsKey("unityUnitsPerLongitude")) {
                    location.UnityUnitsPerLongitude = System.Convert.ToDouble(feature.Properties["unityUnitsPerLongitude"]);
                }
                if (feature.Properties.ContainsKey("unityUnitsPerLatitude")) {
                    location.UnityUnitsPerLatitude = System.Convert.ToDouble(feature.Properties["unityUnitsPerLatitude"]);
                }
                if (feature.Properties.ContainsKey("unityUnitsPerMeters")) {
                    location.UnityUnitsPerMeters = System.Convert.ToDouble(feature.Properties["unityUnitsPerMeters"]);
                }
                if (feature.Properties.ContainsKey("worldRelativeScale")) {
                    location.WorldRelativeScale = (float) System.Convert.ToDouble(feature.Properties["worldRelativeScale"]);
                }
                environmentManager.AddLocation(location);
            } else if (feature.Properties.ContainsKey("type") && string.Equals(feature.Properties["type"] as string, "camera")) {
                atLeastOneCameraCreated = true;

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

                camerasManager.CreateCamera(new CameraNode(name, latLong, altitude, eulerAngles, cameraParameters));
            } else {
                GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
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
                if (feature.Properties.ContainsKey("LightPrefabName")) {
                    prefabName = feature.Properties["LightPrefabName"] as string;
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

        if (!atLeastOneCameraCreated) {
            camerasManager.CreateDefaultCamera();
        }
    }

    private GeoJSON.Net.Feature.FeatureCollection ReadFile(string filename)
    {
        StreamReader rd = new StreamReader(filename);
        GeoJSON.Net.Feature.FeatureCollection featureCollection = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(rd.ReadToEnd());
        rd.Close();
        return featureCollection;
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