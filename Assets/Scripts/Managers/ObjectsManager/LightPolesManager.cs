using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LightPolesManager : MonoBehaviour, IObjectsManager
{
    [SerializeField] private LightPolesGroupsManager lightPolesGroupsManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private IESManager iesManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private LightPolesControl lightPolesControl;
    [SerializeField] private GameObject selectionPinPrefab;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private List<GameObject> lightPolePrefabs;
    private List<LightPole> lightPoles;
    private List<(LightPole, SelectionPin)> selectedLightPoles;
    private List<string> lightPrefabNames;

    void Awake()
    {
        Assert.IsNotNull(lightPolesGroupsManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(lightPolesControl);
        Assert.IsNotNull(selectionPinPrefab);
        Assert.IsNotNull(highlightMaterial);
        Assert.IsNotNull(lightPolePrefabs);
        
        lightPoles = new List<LightPole>();
        selectedLightPoles = new List<(LightPole, SelectionPin)>();

        lightPrefabNames = new List<string>();
        foreach (GameObject lightPolePrefab in lightPolePrefabs) {
            lightPrefabNames.Add(lightPolePrefab.name);
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
        lightPolesGroupsManager.Clear();
    }

    public void OnLocationChanged()
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.SetPosition(mapManager.GetUnityPositionFromCoordinates(lightPole.Coordinate, true));
            lightPole.Light.Show(mapManager.IsCoordinateOnMap(lightPole.Coordinate));
        }

        foreach (var item in selectedLightPoles) {
            item.Item2.SetPosition(item.Item1.Light.transform.position);
        }
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (LightPole lightPole in lightPoles) {
            // Only save light poles of the main configuration
            if (lightPole.ConfigurationIndex == 0) {
                GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                    lightPole.Coordinate.latitude,
                    lightPole.Coordinate.longitude,
                    lightPole.Coordinate.altitude
                ));
                
                Vector3 eulerAngles = lightPole.Light.transform.eulerAngles;
                
                Dictionary<string, object> properties = new Dictionary<string, object>() {
                    {"type", "light"},
                    {"name", lightPole.Name},
                    {"eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z}},
                    {"IESfileName", lightPole.Light.GetIESLight().Name},
                    {"prefabName", lightPole.PrefabName},
                    {"height", lightPole.Light.GetHeight()},
                    {"groups", lightPole.Groups}
                };

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
            
            List<string> newGroups = new List<string>();

            // Groups are only relevant for the main configuration
            if (configurationIndex == 0) {
                try {
                    newGroups = (feature.Properties["groups"] as Newtonsoft.Json.Linq.JArray).ToObject<List<string>>();
                } catch (System.Exception) {}

                lightPolesGroupsManager.AddGroups(newGroups);
            }

            LightPole lightPole = new LightPole(
                new Coordinate(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude),
                configurationIndex,
                newGroups
            );

            if (feature.Properties.ContainsKey("name")) {
                lightPole.Name = feature.Properties["name"] as string;
            }

            if (feature.Properties.ContainsKey("prefabName")) {
                lightPole.PrefabName = feature.Properties["prefabName"] as string;
            }

            float height = LightPrefab.DEFAULT_HEIGHT;
            try {
                // The property must be converted to double first, otherwise an error occurs
                height = (float) (double) feature.Properties["height"];
            } catch (System.Exception) {}

            List<float> eulerAngles = new List<float>();
            try {
                eulerAngles = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
            } catch (System.Exception) {}
            if (eulerAngles.Count < 3) {
                eulerAngles = new List<float>{ 0, 0, 0 };
            }

            string IESName = "";
            if (feature.Properties.ContainsKey("IESfileName")) {
                IESName = feature.Properties["IESfileName"] as string;
            }

            CreateLight(lightPole, height, new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]), IESName);
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
                    // Only Point or MultiPoint are supported
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

        if (configurationIndex == 0) {
            lightPolesGroupsManager.Clear();
        }
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

    public void SelectLightPolePointerByCursor(bool addToSelected)
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

    public void ChangeSelectedLightPolesHeight(float height)
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Light.SetHeight(height);
        }
    }

    public void RotateSelectedLightPoles(float rotation)
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Light.Rotate(rotation);
        }
    }

    public void AddGroupToSelectedLightPoles(string group)
    {
        foreach (var item in selectedLightPoles) {
            if (!item.Item1.Groups.Contains(group)) {
                item.Item1.Groups.Add(group);
            }
        }
    }

    public void RemoveGroupFromSelectedLightPoles(string group)
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Groups.Remove(group);
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
            float height = item.Item1.Light.GetHeight();
            Vector3 eulerAngles = item.Item1.Light.transform.eulerAngles;
            IESLight iesLight = item.Item1.Light.GetIESLight();

            item.Item1.Light.Destroy();
            item.Item1.PrefabName = model;

            CreateLightPrefab(item.Item1, height, eulerAngles, iesLight);
        }
    }

    public void AddLightPole()
    {
        LightPole lightPole = new LightPole();
        CreateLight(lightPole, LightPrefab.DEFAULT_HEIGHT, new Vector3(0, 0), "");
        AddLightPoleToSelected(lightPole);
        MoveSelectedLightPoles();
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

        lightPolesGroupsManager.SetGroupsFromLightPoles(lightPoles);
        ClearSelectedLightPoles();
    }

    public void HighlightLights(bool hightlight)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Highlight(hightlight, highlightMaterial);
        }
    }

    public void ShowLightPoles(bool show)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Light.Show(show && mapManager.IsCoordinateOnMap(lightPole.Coordinate));
        }
    }

    public void ClearSelectedLightPoles()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Coordinate = mapManager.GetCoordinatesFromUnityPosition(item.Item1.Light.transform.position);
            item.Item1.Light.SetMoving(false);
            Destroy(item.Item2.gameObject);
        }
        selectedLightPoles.Clear();
        lightPolesControl.ClearSelectedLights();
    }

    public void SelectFromGroup(string group)
    {
        ClearSelectedLightPoles();

        foreach (LightPole lightPole in lightPoles) {
            if (lightPole.Groups.Contains(group)) {
                AddLightPoleToSelected(lightPole, true);
            }
        }

        lightPolesControl.SetCurrentGroup(group);
    }

    public List<string> GetLightPrefabNames()
    {
        return lightPrefabNames;
    }

    public List<LightPole> GetLightPoles()
    {
        return lightPoles;
    }

    private void CreateLight(LightPole lightPole, float height, Vector3 eulerAngles, string IESName)
    {
        if (lightPole.Name == "") {
            lightPole.Name = Utils.DetermineNewName(lightPoles.Select(light => light.Name).ToList(), "Light");
        }

        if (!lightPrefabNames.Contains(lightPole.PrefabName)) {
            lightPole.PrefabName = lightPrefabNames[0];
        }

        CreateLightPrefab(lightPole, height, eulerAngles, iesManager.GetIESLightFromName(IESName));

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

            lightPolesControl.LightSelected(lightPole, selectedLightPoles.Count > 1);
        }
    }

    private void CreateLightPrefab(LightPole lightPole, float height, Vector3 eulerAngles, IESLight iESLight)
    {
        GameObject lightPrefab = null;
        foreach (GameObject light in lightPolePrefabs) {
            if (light.name == lightPole.PrefabName) {
                lightPrefab = light;
            }
        }
        if (lightPrefab == null) {
            lightPrefab = lightPolePrefabs[0];
        }

        lightPole.Light = Instantiate(
            lightPrefab,
            mapManager.GetUnityPositionFromCoordinates(lightPole.Coordinate, true),
            Quaternion.Euler(eulerAngles),
            transform
        ).GetComponent<LightPrefab>();

        lightPole.Light.Create(sceneCamerasManager);
        lightPole.Light.SetIESLight(iESLight);
        lightPole.Light.SetHeight(height);
        lightPole.Light.Highlight(lightPolesControl.IsHighlighted(), highlightMaterial);
        lightPole.Light.ShowLight(lightPole.ConfigurationIndex == 0);
        lightPole.Light.Show(mapManager.IsCoordinateOnMap(lightPole.Coordinate));
    }
}