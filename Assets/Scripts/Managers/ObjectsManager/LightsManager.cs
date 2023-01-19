using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LightsManager : MonoBehaviour, IObjectsManager
{
    private const string LIGHTS_RESOURCES_FOLDER = "Lights";
    [SerializeField] private MapManager mapManager;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private IESManager iesManager;
    [SerializeField] private LightControl lightControl;
    [SerializeField] private DialogControl dialogControl;
    [SerializeField] private SelectionPin selectionPin;
    [SerializeField] private Material highlightMaterial;
    private List<LightPole> lightPoles;
    private LightPole selectedLightPole;
    private List<string> lightPrefabNames;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(lightControl);
        Assert.IsNotNull(dialogControl);
        Assert.IsNotNull(selectionPin);
        Assert.IsNotNull(highlightMaterial);
        
        lightPoles = new List<LightPole>();
        selectedLightPole = null;

        lightPrefabNames = new List<string>();
        Object[] lights = Resources.LoadAll(LIGHTS_RESOURCES_FOLDER);
        foreach (Object light in lights) {
            lightPrefabNames.Add(light.name);
        }
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        GeoJSON.Net.Geometry.Point point = null;
        if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Point")) {
            point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        } else if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiPoint")) {
            point = (feature.Geometry as GeoJSON.Net.Geometry.MultiPoint).Coordinates[0];
        }

        if (point != null) {
            double altitude = 0;
            if (point.Coordinates.Altitude != null) {
                altitude = (double) point.Coordinates.Altitude;
            }
            LightPole lightPole = new LightPole(new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude));

            if (feature.Properties.ContainsKey("name")) {
                lightPole.Name = feature.Properties["name"] as string;
            }

            if (feature.Properties.ContainsKey("prefabName")) {
                lightPole.PrefabName = feature.Properties["prefabName"] as string;
            }

            List<float> eulerAngles = new List<float>();
            try
            {
                eulerAngles = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
            }
            catch (System.Exception)
            {}
            if (eulerAngles.Count < 3) {
                eulerAngles = new List<float>{ 0, 0, 0 };
            }

            string IESName = "";
            if (feature.Properties.ContainsKey("IESfileName")) {
                IESName = feature.Properties["IESfileName"] as string;
            }

            CreateLight(lightPole, new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]), IESName);
        }
    }

    public void Clear()
    {
        ClearSelectedLight();
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Destroy();
        }
        lightPoles.Clear();
    }

    public void OnLocationChanged()
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.SetPosition(mapManager.GetUnityPositionFromCoordinates(lightPole.Coordinates, true));
        }

        if (selectedLightPole != null) {
            selectionPin.SetPosition(selectedLightPole.Light.GetTransform().position);
        }
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (LightPole lightPole in lightPoles) {
            GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                lightPole.Coordinates.latitude,
                lightPole.Coordinates.longitude,
                lightPole.Coordinates.altitude
            ));
            
            Vector3 eulerAngles = lightPole.Light.GetTransform().eulerAngles;
            
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("type", "light");
            properties.Add("name", lightPole.Name);
            properties.Add("eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z});
            properties.Add("IESfileName", lightPole.Light.GetIESLight().Name);
            properties.Add("prefabName", lightPole.PrefabName);

            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
        }

        return features;
    }

    public void Create()
    {
        LightPole lightPole = new LightPole();
        CreateLight(lightPole, new Vector3(0, 0), "");
        SelectLight(lightPole);
        MoveCurrentLight();
    }

    public void AddLightsFromFile()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Insert lights to the scene", "", "geojson", true);
        foreach (string path in paths) {
            string message = "";

            try {
                GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(path);

                bool atLeastOneValidFeature = false;

                foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                    GeoJSON.Net.Geometry.Point point = null;
                    if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Point")) {
                        point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                    } else if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiPoint")) {
                        point = (feature.Geometry as GeoJSON.Net.Geometry.MultiPoint).Coordinates[0];
                    }

                    if (point != null && Utils.IsEPSG4326(point.Coordinates)) {
                        atLeastOneValidFeature = true;
                        Create(feature);
                    }
                }

                if (atLeastOneValidFeature) {
                    message = "Lights added.";
                } else {
                    message = "Lights not added.\n";
                    message += "The GeoJSON file should be made of Point or MultiPoint.\n";
                    message += "The EPSG:4326 coordinate system should be used (longitude from -180° to 180° / latitude from -90° to 90°).";
                }
            } catch (System.Exception e) {
                message = e.Message;
            }
            
            dialogControl.CreateInfoDialog(message);
        }
    }

    public void DeleteLight()
    {
        if (selectedLightPole != null) {
            selectedLightPole.Light.Destroy();
            lightPoles.Remove(selectedLightPole);
            ClearSelectedLight();
        }
    }

    public void ShowLights(bool show)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Show(show);
        }
    }

    public void ChangeLightType(string newLightType)
    {
        if (selectedLightPole != null && newLightType != selectedLightPole.PrefabName) {
            Vector3 eulerAngles = selectedLightPole.Light.GetTransform().eulerAngles;
            IESLight iesLight = selectedLightPole.Light.GetIESLight();
            selectedLightPole.Light.Destroy();

            selectedLightPole.PrefabName = newLightType;
            selectedLightPole.Light = Instantiate(Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + newLightType)).GetComponent<LightPrefab>();
            selectedLightPole.Light.Create(selectedLightPole, transform, eulerAngles, mapManager);
            selectedLightPole.Light.SetIESLight(iesLight);
        }
    }

    public void MoveLight()
    {
        if (selectedLightPole != null) {
            if (selectedLightPole.Light.IsMoving()) {
                ClearSelectedLight();
            } else {
                MoveCurrentLight();
            }
        }
    }

    public void RotateSelectedLight(float rotation)
    {
        if (selectedLightPole != null) {
            selectedLightPole.Light.Rotate(rotation);
        }
    }

    public void ChangeLightSource(string newIESName)
    {
        if (selectedLightPole != null && newIESName != selectedLightPole.Light.GetIESLight().Name) {
            IESLight newIES = iesManager.GetIESLightFromName(newIESName);

            if (newIES != null) {
                selectedLightPole.Light.SetIESLight(newIES);
            }
        }
    }

    public void SelectLight()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 10000)) {
            LightPrefab lightPrefab = hitInfo.transform.gameObject.GetComponent<LightPrefab>();
            if (lightPrefab != null) {
                foreach (LightPole lightPole in lightPoles) {
                    if (lightPole.Light == lightPrefab) {
                        selectedLightPole = lightPole;
                        SelectLight(selectedLightPole);
                    }
                }
            }
        }
    }

    public void ClearSelectedLight()
    {
        if (selectedLightPole != null) {
            selectedLightPole.Light.SetMoving(false);

            Vector3 lightPosition = selectedLightPole.Light.GetTransform().position;
            selectedLightPole.Coordinates = mapManager.GetCoordinatesFromUnityPosition(lightPosition);

            selectedLightPole = null;
            selectionPin.SetActive(false);
        }
        lightControl.ClearSelectedLight();
    }

    public void HighlightLights(bool hightlight)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Hightlight(hightlight, highlightMaterial);
        }
    }

    public List<string> GetLightPrefabNames()
    {
        return lightPrefabNames;
    }

    private void CreateLight(LightPole lightPole, Vector3 eulerAngles, string IESName)
    {
        if (lightPole.Name == "") {
            lightPole.Name = Utils.DetermineNewName(lightPoles.Select(light => light.Name).ToList(), "Light");
        }

        if (!lightPrefabNames.Contains(lightPole.PrefabName)) {
            lightPole.PrefabName = lightPrefabNames[0];
        }
        
        lightPole.Light = Instantiate(Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + lightPole.PrefabName)).GetComponent<LightPrefab>();
        lightPole.Light.Create(lightPole, transform, eulerAngles, mapManager);
        lightPole.Light.SetIESLight(iesManager.GetIESLightFromName(IESName));

        lightPoles.Add(lightPole);
    }

    private void SelectLight(LightPole lightPole)
    {
        ClearSelectedLight();
        selectedLightPole = lightPole;

        lightControl.LightSelected(selectedLightPole);
        
        selectionPin.SetActive(true);
        selectionPin.SetPosition(selectedLightPole.Light.GetTransform().position);
    }

    private void MoveCurrentLight()
    {
        selectedLightPole.Light.SetMoving(true);
        selectionPin.SetMoving(true);
    }
}