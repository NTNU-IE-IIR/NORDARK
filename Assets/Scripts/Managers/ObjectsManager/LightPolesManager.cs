using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LightPolesManager : ObjectsManager
{
    [SerializeField] private LightPolesGroupsManager lightPolesGroupsManager;
    [SerializeField] private TerrainManager terrainManager;
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
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(lightPolesGroupsManager);
        Assert.IsNotNull(terrainManager);
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

    public void Create(GeoJSON.Net.Feature.Feature feature, int configurationIndex, Location location)
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
                location,
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

    public void AddLightPolesFromGeoJSONFile()
    {
        Location currentLocation = locationsManager.GetCurrentLocation();
        if (currentLocation != null) {
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
                        feature.Properties.Add("location", currentLocation.Name);

                        if (point != null && Utils.IsEPSG4326(point.Coordinates)) {
                            atLeastOneValidFeature = true;
                            Create(feature);
                        }
                    }

                    if (atLeastOneValidFeature) {
                        message = "Lights from " + path + " added.";
                    } else {
                        message = "Lights from " + path + " not added.";
                        message += "The GeoJSON file should be made of Point or MultiPoint.\n";
                        message += "The EPSG:4326 coordinate system should be used (longitude from -180° to 180° / latitude from -90° to 90°).";
                    }
                } catch (System.Exception e) {
                    message = e.Message;
                }
                
                DialogControl.CreateDialog(message);
            }
        }
    }

    public void AddLightPolesFromCSVFile()
    {
        Location currentLocation = locationsManager.GetCurrentLocation();
        if (currentLocation != null) {
            string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Insert lights to the scene", "", "csv", true);
            foreach (string path in paths) {
                string message;

                string[] lines = System.IO.File.ReadAllLines(path);
                try {
                    for (int i=1; i<lines.Length; ++i) {
                        string[] values = lines[i].Split(',');
                        GeoJSON.Net.Geometry.Point point = new GeoJSON.Net.Geometry.Point(
                            new GeoJSON.Net.Geometry.Position(float.Parse(values[0]), float.Parse(values[1]))
                        );
                        Create(new GeoJSON.Net.Feature.Feature(
                            new GeoJSON.Net.Geometry.Point(
                                new GeoJSON.Net.Geometry.Position(float.Parse(values[0]), float.Parse(values[1]))
                            ), new Dictionary<string, object>() {
                                {"type", "light"},
                                {"location", currentLocation.Name}
                            }
                        ));
                    }
                    message = "Lights from " + path + " added.";
                } catch (System.Exception e) {
                    message = e.Message;
                }
                
                DialogControl.CreateDialog(message);
            }
        }
    }

    public void DeleteAllLightPolesFromConfiguration(int configurationIndex)
    {
        foreach (LightPole lightPole in lightPoles) {
            if (lightPole.ConfigurationIndex == configurationIndex) {
                lightPole.GameObject.Destroy();
            }
        }

        lightPoles.RemoveAll(lightPole => lightPole.ConfigurationIndex == configurationIndex);

        if (configurationIndex == 0) {
            lightPolesGroupsManager.Clear();
        }
    }

    public void AddLightPrefabToSelectedLightPoles(LightPrefab lightPrefab, bool addToSelected)
    {
        if (lightPrefab != null) {
            foreach (LightPole lightPole in lightPoles) {
                if (lightPole.GameObject == lightPrefab && lightPole.ConfigurationIndex == 0) {
                    AddLightPoleToSelected(lightPole, addToSelected);
                }
            }
        }
    }

    public void StopLightPolesMovement()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.Coordinate = terrainManager.GetCoordinatesFromUnityPosition(item.Item1.GameObject.transform.position);
            item.Item1.GameObject.SetMoving(false);
            item.Item2.SetMoving(false);
        }
    }

    public LightPole GetLightPolePointedByCursor()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
            LightPrefab lightPrefab = hitInfo.transform.gameObject.GetComponent<LightPrefab>();
            if (lightPrefab != null) {
                foreach (LightPole lightPole in lightPoles) {
                    if (lightPole.GameObject == lightPrefab && lightPole.ConfigurationIndex == 0) {
                        return lightPole;
                    }
                }
            }
        }
        return null;
    }

    public void SelectLightPolePointedByCursor(bool addToSelected)
    {
        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
            AddLightPrefabToSelectedLightPoles(
                hitInfo.transform.gameObject.GetComponent<LightPrefab>(),
                addToSelected
            );
        }
    }

    public void SelectLightPolesWithinPositions(Vector3 positionA, Vector3 positionB)
    {
        ClearSelectedObjects();

        int xMin = (int) (positionA.x < positionB.x ? positionA.x : positionB.x);
        int xMax = (int) (positionA.x < positionB.x ? positionB.x : positionA.x);
        int zMin = (int) (positionA.z < positionB.z ? positionA.z : positionB.z);
        int zMax = (int) (positionA.z < positionB.z ? positionB.z : positionA.z);

        foreach (LightPole lightPole in lightPoles) {
            Vector3 position = lightPole.GameObject.transform.position;

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
            item.Item1.GameObject.SetHeight(height);
        }
    }

    public void RotateSelectedObjects(float rotation)
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.GameObject.Rotate(rotation);
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
                item.Item1.GameObject.SetIESLight(iesLight);
            }
        }
    }

    public void Change3DModelOfSelectedLightPoles(string model)
    {
        foreach (var item in selectedLightPoles) {
            float height = item.Item1.GameObject.GetHeight();
            Vector3 eulerAngles = item.Item1.GameObject.transform.eulerAngles;
            IESLight iesLight = item.Item1.GameObject.GetIESLight();

            item.Item1.GameObject.Destroy();
            item.Item1.PrefabName = model;

            CreateLightPrefab(item.Item1, height, eulerAngles, iesLight);
        }
    }

    public void AddLightPole()
    {
        LightPole lightPole = new LightPole(locationsManager.GetCurrentLocation());
        CreateLight(lightPole, LightPrefab.DEFAULT_HEIGHT, new Vector3(0, 0), "");
        AddLightPoleToSelected(lightPole, false);
        MoveSelectedObjects();
    }

    public void MoveSelectedObjects()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.GameObject.Show(true);
            item.Item1.GameObject.SetMoving(true);
            item.Item2.SetMoving(true);
        }
    }

    public void DeleteSelectedObjects()
    {
        foreach (var item in selectedLightPoles) {
            item.Item1.GameObject.Destroy();
            lightPoles.Remove(item.Item1);
        }

        lightPolesGroupsManager.SetGroupsFromLightPoles(lightPoles);
        ClearSelectedObjects();
    }

    public void HighlightLights(bool hightlight)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.GameObject.Highlight(hightlight, highlightMaterial);
        }
    }

    public void ShowObjects(bool show)
    {
        foreach (LightPole lightPole in lightPoles) {
            lightPole.GameObject.Show(show && terrainManager.IsCoordinateOnMap(lightPole.Coordinate));
        }
    }

    public void ClearSelectedObjects()
    {
        StopLightPolesMovement();
        
        foreach (var item in selectedLightPoles) {
            Destroy(item.Item2.gameObject);
        }
        selectedLightPoles.Clear();
        lightPolesControl.ClearSelectedLights();
    }

    public void SelectFromGroup(string group)
    {
        ClearSelectedObjects();

        foreach (LightPole lightPole in lightPoles) {
            if (lightPole.Groups.Contains(group)) {
                AddLightPoleToSelected(lightPole, true);
            }
        }

        lightPolesControl.SetCurrentGroup(group);
    }

    public List<string> GetObjectPrefabNames()
    {
        return lightPrefabNames;
    }

    public List<LightPole> GetLightPoles()
    {
        return lightPoles;
    }

    protected override void CreateObject(GeoJSON.Net.Feature.Feature feature, Location location)
    {
        Create(feature, 0, location);
    }

    protected override void ClearActiveObjects()
    {
        ClearSelectedObjects();

        foreach (LightPole lightPole in lightPoles) {
            lightPole.GameObject.Destroy();
        }
        lightPoles.Clear();
        lightPolesGroupsManager.Clear();
    }

    protected override void OnAfterLocationChanged()
    {}

    protected override List<GeoJSON.Net.Feature.Feature> GetFeaturesOfCurrentLocation()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (LightPole lightPole in lightPoles) {
            if (lightPole.Location != null) {

                // Only save light poles of the main configuration
                if (lightPole.ConfigurationIndex == 0) {
                    GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                        lightPole.Coordinate.latitude,
                        lightPole.Coordinate.longitude,
                        lightPole.Coordinate.altitude
                    ));
                    
                    Vector3 eulerAngles = lightPole.GameObject.transform.eulerAngles;
                    
                    Dictionary<string, object> properties = new Dictionary<string, object>() {
                        {"type", "light"},
                        {"location", lightPole.Location.Name},
                        {"name", lightPole.Name},
                        {"eulerAngles", Newtonsoft.Json.Linq.JArray.FromObject(new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z})},
                        {"IESfileName", lightPole.GameObject.GetIESLight().Name},
                        {"prefabName", lightPole.PrefabName},
                        {"height", lightPole.GameObject.GetHeight()},
                        {"groups", Newtonsoft.Json.Linq.JArray.FromObject(lightPole.Groups)}
                    };

                    features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
                }
            }
        }

        return features;
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
        if (!addToSelected) {
            ClearSelectedObjects();
        }

        SelectionPin selectionPin = Instantiate(selectionPinPrefab, transform).GetComponent<SelectionPin>();
        selectionPin.Create(sceneCamerasManager);
        selectionPin.SetPosition(lightPole.GameObject.transform.position);
        selectedLightPoles.Add((lightPole, selectionPin));

        lightPolesControl.LightSelected(lightPole, selectedLightPoles.Count > 1);
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

        lightPole.GameObject = Instantiate(
            lightPrefab,
            terrainManager.GetUnityPositionFromCoordinates(lightPole.Coordinate, true),
            Quaternion.Euler(eulerAngles),
            transform
        ).GetComponent<LightPrefab>();

        lightPole.GameObject.Create(sceneCamerasManager);
        lightPole.GameObject.SetIESLight(iESLight);
        lightPole.GameObject.SetHeight(height);
        lightPole.GameObject.Highlight(lightPolesControl.IsHighlighted(), highlightMaterial);
        lightPole.GameObject.ShowLight(lightPole.ConfigurationIndex == 0);
        lightPole.GameObject.Show(terrainManager.IsCoordinateOnMap(lightPole.Coordinate));
    }
}