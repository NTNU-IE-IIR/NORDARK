using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LightPolesManager : MonoBehaviour, IObjectsManager
{
    private const string LIGHTS_RESOURCES_FOLDER = "Lights";
    [SerializeField] private MapManager mapManager;
    [SerializeField] private IESManager iesManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private LightControl lightControl;
    [SerializeField] private LightsTabControl lightsTabControl;
    [SerializeField] private GameObject selectionPinPrefab;
    [SerializeField] private Material highlightMaterial;
    private List<LightPole> lightPoles;
    private List<(LightPole, SelectionPin)> selectedLightPoles;
    private List<string> lightPrefabNames;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(lightControl);
        Assert.IsNotNull(lightsTabControl);
        Assert.IsNotNull(selectionPinPrefab);
        Assert.IsNotNull(highlightMaterial);
        
        lightPoles = new List<LightPole>();
        selectedLightPoles = new List<(LightPole, SelectionPin)>();

        lightPrefabNames = new List<string>();
        Object[] lights = Resources.LoadAll(LIGHTS_RESOURCES_FOLDER);
        foreach (Object light in lights) {
            lightPrefabNames.Add(light.name);
        }
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        Create(feature, 0);
    }

    public void Clear()
    {
        ClearSelectedLightPoles();

        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Destroy();
        }
        lightPoles.Clear();
    }

    public void OnLocationChanged()
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.SetPosition(mapManager.GetUnityPositionFromCoordinates(lightPole.Coordinates, true));
            lightPole.Light.Show(mapManager.IsCoordinateOnMap(lightPole.Coordinates));
        }

        foreach (var item in selectedLightPoles) {
            item.Item2.SetPosition(item.Item1.Light.transform.position);
        }
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (LightPole lightPole in lightPoles) {
            if (lightPole.ConfigurationIndex == 0) {
                GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                    lightPole.Coordinates.latitude,
                    lightPole.Coordinates.longitude,
                    lightPole.Coordinates.altitude
                ));
                
                Vector3 eulerAngles = lightPole.Light.transform.eulerAngles;
                
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add("type", "light");
                properties.Add("name", lightPole.Name);
                properties.Add("eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z});
                properties.Add("IESfileName", lightPole.Light.GetIESLight().Name);
                properties.Add("prefabName", lightPole.PrefabName);

                features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
            }            
        }

        return features;
    }

    public void Create(GeoJSON.Net.Feature.Feature feature, int configurationIndex)
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
            LightPole lightPole = new LightPole(new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude), configurationIndex);

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

    public void AddLightPolesFromFile()
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
            
            DialogControl.CreateDialog(message);
        }
    }

    public void DeleteAllLightPolesFromConfiguration(int configurationIndex)
    {
        foreach (LightPole lightPole in lightPoles) {
            if (lightPole.ConfigurationIndex == configurationIndex) {
                lightPole.Light.Destroy();
            }
        }

        lightPoles.RemoveAll(lightPole => lightPole.ConfigurationIndex == configurationIndex);
    }

    public bool AddLightPrefabToSelectedLightPoles(LightPrefab lightPrefab, bool addToSelected)
    {
        if (lightPrefab != null) {
            foreach (LightPole lightPole in lightPoles) {
                if (lightPole.Light == lightPrefab && lightPole.ConfigurationIndex == 0) {
                    AddLightPoleToSelected(lightPole, addToSelected);
                    return true;
                }
            }
        }
        return false;
    }

    public void SelectLightPoleFromPointerByCursor(bool addToSelected)
    {
        bool lightPoleSelected = false;

        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
            lightPoleSelected = lightPoleSelected || AddLightPrefabToSelectedLightPoles(
                hitInfo.transform.gameObject.GetComponent<LightPrefab>(),
                addToSelected
            );
        }

        if (!lightPoleSelected) {
            foreach (var item in selectedLightPoles) {
                item.Item1.Light.SetMoving(false);
                item.Item2.SetMoving(false);
            }
        }
    }

    public void SelectLightPolesWithinPositions(Vector3 positionA, Vector3 positionB)
    {
        int xMin = (int) (positionA.x < positionB.x ? positionA.x : positionB.x);
        int xMax = (int) (positionA.x < positionB.x ? positionB.x : positionA.x);
        int zMin = (int) (positionA.z < positionB.z ? positionA.z : positionB.z);
        int zMax = (int) (positionA.z < positionB.z ? positionB.z : positionA.z);

        foreach (LightPole lightPole in lightPoles) {
            Vector3 position = lightPole.Light.transform.position;

            if (position.x > xMin && position.x < xMax && position.z > zMin && position.z < zMax) {
                AddLightPoleToSelected(lightPole);
            }
        }
    }

    public void ChangeSelectedLightPolesName(string name)
    {
        if (selectedLightPoles.Count == 1) {
            selectedLightPoles[0].Item1.Name = name;
        }
    }

    public void RotateSelectedLightPoles(float rotation)
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Light.Rotate(rotation);
        }
    }

    public void ChangeIESFileOfSelectedLightPoles(string iesFile)
    {
        IESLight iesLight = iesManager.GetIESLightFromName(iesFile);
        if (iesLight != null) {
            foreach (var item in selectedLightPoles) {
                item.Item1.Light.SetIESLight(iesLight);
            }
        }
    }

    public void Change3DModelOfSelectedLightPoles(string model)
    {
        foreach (var item in selectedLightPoles) {
            Vector3 eulerAngles = item.Item1.Light.transform.eulerAngles;
            IESLight iesLight = item.Item1.Light.GetIESLight();

            item.Item1.Light.Destroy();
            item.Item1.PrefabName = model;

            CreateLightPrefab(item.Item1, eulerAngles, iesLight);
        }
    }

    public void AddLightPole()
    {
        if (lightsTabControl.IsActive()) {
            LightPole lightPole = new LightPole(mapManager.GetCoordinatesFromUnityPosition(new Vector3()), 0);
            CreateLight(lightPole, new Vector3(0, 0), "");
            AddLightPoleToSelected(lightPole);
            MoveSelectedLightPoles();
        }
    }

    public void MoveSelectedLightPoles()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Light.SetMoving(!item.Item1.Light.IsMoving());
            item.Item2.SetMoving(item.Item1.Light.IsMoving());
        }
    }

    public void DeleteSelectedLightPoles()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Light.Destroy();
            lightPoles.Remove(item.Item1);
        }

        ClearSelectedLightPoles();
    }

    public void HighlightLights(bool hightlight)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Hightlight(hightlight, highlightMaterial);
        }
    }

    public void ShowLightPoles(bool show)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Show(show && mapManager.IsCoordinateOnMap(lightPole.Coordinates));
        }
    }

    public void ClearSelectedLightPoles()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Coordinates = mapManager.GetCoordinatesFromUnityPosition(item.Item1.Light.transform.position);
            item.Item1.Light.SetMoving(false);
            Destroy(item.Item2.gameObject);
        }
        selectedLightPoles.Clear();
        lightControl.ClearSelectedLight();
    }

    public List<string> GetLightPrefabNames()
    {
        return lightPrefabNames;
    }

    public List<LightPole> GetLights()
    {
        return lightPoles;
    }

    private void CreateLight(LightPole lightPole, Vector3 eulerAngles, string IESName)
    {
        if (lightPole.Name == "") {
            lightPole.Name = Utils.DetermineNewName(lightPoles.Select(light => light.Name).ToList(), "Light");
        }

        if (!lightPrefabNames.Contains(lightPole.PrefabName)) {
            lightPole.PrefabName = lightPrefabNames[0];
        }

        CreateLightPrefab(lightPole, eulerAngles, iesManager.GetIESLightFromName(IESName));

        lightPoles.Add(lightPole);
    }

    private void AddLightPoleToSelected(LightPole lightPole, bool addToSelected = true)
    {
        if (selectedLightPoles.Select(item => item.Item1).Contains(lightPole)) {
            lightPole.Light.SetMoving(false);
            selectedLightPoles.Find(item => item.Item1 == lightPole).Item2.SetMoving(false);
        } else {
            if (!addToSelected) {
                ClearSelectedLightPoles();
            }

            SelectionPin selectionPin = Instantiate(selectionPinPrefab, transform).GetComponent<SelectionPin>();
            selectionPin.Create(sceneCamerasManager);
            selectionPin.SetPosition(lightPole.Light.transform.position);
            selectedLightPoles.Add((lightPole, selectionPin));

            lightControl.LightSelected(lightPole, selectedLightPoles.Count > 1);
        }
    }

    private void CreateLightPrefab(LightPole lightPole, Vector3 eulerAngles, IESLight iESLight)
    {
        lightPole.Light = Instantiate(
            Resources.Load<GameObject>(LIGHTS_RESOURCES_FOLDER + "/" + lightPole.PrefabName),
            mapManager.GetUnityPositionFromCoordinates(lightPole.Coordinates, true),
            Quaternion.Euler(eulerAngles),
            transform
        ).GetComponent<LightPrefab>();

        lightPole.Light.Create(sceneCamerasManager);
        lightPole.Light.SetIESLight(iESLight);
        lightPole.Light.Hightlight(lightControl.IsHighlighted(), highlightMaterial);
        lightPole.Light.ShowLight(lightPole.ConfigurationIndex == 0);
        lightPole.Light.Show(mapManager.IsCoordinateOnMap(lightPole.Coordinates));
    }
}