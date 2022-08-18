using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using GeoJSON.Net.Feature;
using SFB;

public class SceneManager : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private MainCameraManager mainCameraManager;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(mainCameraManager);
    }

    void Start()
    {
        FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(Resources.Load<TextAsset>("defaultScene").text);
        LoadScene(featureCollection);
    }

    public void Load()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a NORDARK scene file", "", "geojson", false);
        if (paths.Length > 0)
        {
            FeatureCollection featureCollection = ReadFile(paths[0]);
            LoadScene(featureCollection);
        }
    }

    public void Save()
    {
        List<(Mapbox.Utils.Vector2d, Dictionary<string, object>)> features = lightsManager.GetLightFeatures();
        features.Add(camerasManager.GetCameraFeature());

        string filename = StandaloneFileBrowser.SaveFilePanel("Save the current scene", "", "nordark", "geojson");
        if (filename != "")
        {
            SaveToGeojson(filename, features);
        }
    }

    private void LoadScene(FeatureCollection featureCollection)
    {
        lightsManager.ClearLights();
        camerasManager.ClearCameras();

        foreach (Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("mapCenter")) {
                try
                {
                    List<double> mapCenterList = (feature.Properties["mapCenter"] as Newtonsoft.Json.Linq.JArray).ToObject<List<double>>();
                    mapManager.SetMapLocation(new Mapbox.Utils.Vector2d(mapCenterList[0], mapCenterList[1]));
                }
                catch (System.Exception)
                {}

                try
                {
                    List<float> cameraPosList = (feature.Properties["cameraPos"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
                    List<float> cameraAnglesList = (feature.Properties["cameraAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();

                    Vector3 cameraPos = new Vector3(cameraPosList[0], cameraPosList[1], cameraPosList[2]);
                    Vector3 cameraAngles = new Vector3(cameraAnglesList[0], cameraAnglesList[1], cameraAnglesList[2]);
                    mainCameraManager.SetPositionAndEulerAngles(cameraPos, cameraAngles);
                }
                catch (System.Exception)
                {}

                try
                {
                    List<CameraNodeSerialized> cameras = (feature.Properties["cameras"] as Newtonsoft.Json.Linq.JArray).ToObject<List<CameraNodeSerialized>>();
                    if (cameras != null) {
                        camerasManager.CreateCameras(cameras);
                    } else {
                        camerasManager.CreateDefaultCamera();
                    }
                }
                catch (System.Exception)
                {
                    camerasManager.CreateDefaultCamera();
                }
            } else {
                lightsManager.CreateLight(feature);
            }
        }
    }

    private FeatureCollection ReadFile(string filename)
    {
        StreamReader rd = new StreamReader(filename);
        FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>( rd.ReadToEnd());
        rd.Close();
        return featureCollection;
    }

    private void SaveToGeojson(string filename, List<(Mapbox.Utils.Vector2d, Dictionary<string, object>)> features)
    {
        FeatureCollection featureCollection = new FeatureCollection();

        for (int i = 0; i < features.Count; i++) {
            GeoJSON.Net.Geometry.Point point = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(features[i].Item1.x, features[i].Item1.y));

            Feature feature = new Feature(point, features[i].Item2, i.ToString());
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

    private Vector3 StringToVector3(string str)
    {
        string[] strArray = Between(str, "(", ")").Split(',');
        if (strArray.Length == 3) {
            return new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
        } else {
            return new Vector3(0, 0, 0);
        }
    }
    private string Between(string str, string firstString, string lastString)
    {
        int pos1 = str.IndexOf(firstString) + firstString.Length;
        int pos2 = str.IndexOf(lastString);
        return str.Substring(pos1, pos2 - pos1);
    }
    private Mapbox.Utils.Vector2d StringToVector2d(string str)
    {
        string[] strArray = str.Split(',');
        if (strArray.Length == 2) {
            return new Mapbox.Utils.Vector2d(double.Parse(strArray[0]), double.Parse(strArray[1]));
        } else {
            return new Mapbox.Utils.Vector2d(0, 0);
        }
    }
}