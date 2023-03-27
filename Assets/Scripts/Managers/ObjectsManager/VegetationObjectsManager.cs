using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class VegetationObjectsManager : ObjectsManager
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private VegetationObjectsControl vegetationObjectsControl;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private List<GameObject> vegetationObjectsPrefabs;
    [SerializeField] private GameObject selectionPinPrefab;
    private List<VegetationObject> vegetationObjects;
    private List<(VegetationObject, SelectionPin)> selectedObjects;

    void Awake()
    {
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(vegetationObjectsControl);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(vegetationObjectsPrefabs);
        Assert.IsNotNull(selectionPinPrefab);

        vegetationObjects = new List<VegetationObject>();
        selectedObjects = new List<(VegetationObject, SelectionPin)>();
    }

    public List<string> GetObjectPrefabNames()
    {
        return vegetationObjectsPrefabs.Select(vegetationObjectsPrefab => vegetationObjectsPrefab.name).ToList();
    }

    public void StopObjectsMovement()
    {
        foreach (var item in selectedObjects) {
            item.Item1.Coordinate = terrainManager.GetCoordinatesFromUnityPosition(item.Item1.GameObject.transform.position);
            item.Item1.GameObject.SetMoving(false);
            item.Item2.SetMoving(false);
        }
    }

    public void RotateSelectedObjects(float rotation)
    {
        foreach (var item in selectedObjects) {
            item.Item1.GameObject.Rotate(rotation);
        }
        changesUnsaved = true;
    }

    public void Change3DModelOfSelectedObjects(string new3DModelName)
    {
        foreach (var item in selectedObjects) {
            Vector3 eulerAngles = item.Item1.GameObject.transform.eulerAngles;

            item.Item1.GameObject.Destroy();
            item.Item1.PrefabName = new3DModelName;

            CreateVegetationGameObject(item.Item1, eulerAngles);
        }
        changesUnsaved = true;
    }

    public void AddObject()
    {
        VegetationObject vegetationObject = new VegetationObject(locationsManager.GetCurrentLocation());
        CreateVegetationGameObject(vegetationObject, new Vector3());
        vegetationObjects.Add(vegetationObject);

        AddObjectToSelected(vegetationObject, false);
        MoveSelectedObjects();

        changesUnsaved = true;
    }

    public void MoveSelectedObjects()
    {
        foreach (var item in selectedObjects) {
            item.Item1.GameObject.Show(true);
            item.Item1.GameObject.SetMoving(true);
            item.Item2.SetMoving(true);
        }
        changesUnsaved = true;
    }

    public void DeleteSelectedObjects()
    {
        foreach (var item in selectedObjects) {
            item.Item1.GameObject.Destroy();
            vegetationObjects.Remove(item.Item1);
        }

        ClearSelectedObjects();
        changesUnsaved = true;
    }

    public void ShowObjects(bool show)
    {
        foreach (VegetationObject vegetationObject in vegetationObjects) {
            vegetationObject.GameObject.Show(show && terrainManager.IsCoordinateOnMap(vegetationObject.Coordinate));
        }
    }

    public void SelectObjectPointedByCursor(bool addToSelected)
    {
        RaycastHit hitInfo = new RaycastHit();
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI && Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hitInfo)) {
            AddGameObjectToSelectedObjects(
                hitInfo.transform.gameObject.GetComponent<VegetationObjectPrefab>(),
                addToSelected
            );
        }
    }

    public void ClearSelectedObjects()
    {
        StopObjectsMovement();

        foreach (var item in selectedObjects) {
            Destroy(item.Item2.gameObject);
        }
        selectedObjects.Clear();
        vegetationObjectsControl.ClearSelectedObjects();
    }

    protected override void CreateObject(GeoJSON.Net.Feature.Feature feature, Location location)
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

            VegetationObject vegetationObject = new VegetationObject(
                new Coordinate(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude),
                location
            );

            if (feature.Properties.ContainsKey("prefabName")) {
                vegetationObject.PrefabName = feature.Properties["prefabName"] as string;
            }

            List<float> eulerAngles = new List<float>();
            try {
                eulerAngles = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
            } catch (System.Exception) {}
            if (eulerAngles.Count < 3) {
                eulerAngles = new List<float>{ 0, 0, 0 };
            }

            CreateVegetationGameObject(vegetationObject, new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]));

            vegetationObjects.Add(vegetationObject);
        }
    }

    protected override void ClearActiveObjects()
    {
        ClearSelectedObjects();
        
        foreach (VegetationObject vegetationObject in vegetationObjects) {
            vegetationObject.GameObject.Destroy();
        }
        vegetationObjects.Clear();
    }

    protected override void OnAfterLocationChanged()
    {}

    protected override List<GeoJSON.Net.Feature.Feature> GetFeaturesOfCurrentLocation()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (VegetationObject vegetationObject in vegetationObjects) {
            if (vegetationObject.Location != null) {
                GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                    vegetationObject.Coordinate.latitude,
                    vegetationObject.Coordinate.longitude,
                    vegetationObject.Coordinate.altitude
                ));
                
                Vector3 eulerAngles = vegetationObject.GameObject.transform.eulerAngles;
                
                Dictionary<string, object> properties = new Dictionary<string, object>() {
                    {"type", "vegetationObject"},
                    {"location", vegetationObject.Location.Name},
                    {"eulerAngles", Newtonsoft.Json.Linq.JArray.FromObject(new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z})},
                    {"prefabName", vegetationObject.PrefabName}
                };

                features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
            }
        }

        return features;
    }

    private void CreateVegetationGameObject(VegetationObject vegetationObject, Vector3 eulerAngles)
    {
        GameObject vegetationObjectPrefab = null;
        foreach (GameObject objectPrefab in vegetationObjectsPrefabs) {
            if (objectPrefab.name == vegetationObject.PrefabName) {
                vegetationObjectPrefab = objectPrefab;
            }
        }
        if (vegetationObjectPrefab == null) {
            vegetationObjectPrefab = vegetationObjectsPrefabs[0];
            vegetationObject.PrefabName = vegetationObjectPrefab.name;
        }

        vegetationObject.GameObject = Instantiate(
            vegetationObjectPrefab,
            terrainManager.GetUnityPositionFromCoordinates(vegetationObject.Coordinate, true),
            Quaternion.Euler(eulerAngles),
            transform
        ).GetComponent<VegetationObjectPrefab>();

        vegetationObject.GameObject.Create(sceneCamerasManager);
    }

    private void AddGameObjectToSelectedObjects(VegetationObjectPrefab vegetationObjectPrefab, bool addToSelected)
    {
        if (vegetationObjectPrefab != null) {
            foreach (VegetationObject vegetationObject in vegetationObjects) {
                if (vegetationObject.GameObject == vegetationObjectPrefab) {
                    AddObjectToSelected(vegetationObject, addToSelected);
                }
            }
        }
    }

    private void AddObjectToSelected(VegetationObject vegetationObject, bool addToSelected)
    {
        if (!addToSelected) {
            ClearSelectedObjects();
        }
        
        SelectionPin selectionPin = Instantiate(selectionPinPrefab, transform).GetComponent<SelectionPin>();
        selectionPin.Create(sceneCamerasManager);
        selectionPin.SetPosition(vegetationObject.GameObject.transform.position);
        selectedObjects.Add((vegetationObject, selectionPin));

        vegetationObjectsControl.ObjectSelected(vegetationObject, selectedObjects.Count > 1);
    }
}
