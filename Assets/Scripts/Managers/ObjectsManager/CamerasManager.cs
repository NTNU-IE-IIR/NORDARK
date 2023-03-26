using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CamerasManager : ObjectsManager
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private BiomeAreasManager biomeAreasManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private CameraParametersControl cameraParametersControl;
    [SerializeField] private GameObject cameraPreview;
    [SerializeField] private GameObject cameraObject;
    private List<CameraNode> cameraNodes;
    private CameraNode currentCamera;

    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(biomeAreasManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(cameraControl);
        Assert.IsNotNull(cameraParametersControl);
        Assert.IsNotNull(cameraPreview);
        Assert.IsNotNull(cameraObject);

        cameraNodes = new List<CameraNode>();
        currentCamera = null;
    }

    public void CreateCamera()
    {
        CreateCamera(
            new CameraNode(
                Utils.DetermineNewName(cameraNodes.Select(cameraNode => cameraNode.Name).ToList(), "Camera"),
                terrainManager.GetCoordinatesFromUnityPosition(sceneCamerasManager.GetPosition()),
                locationsManager.GetCurrentLocation()
            ),
            sceneCamerasManager.GetEulerAngles(),
            cameraParametersControl.GetCameraParameters()
        );
    }

    public void DeleteCamera()
    {
        if (currentCamera != null) {
            biomeAreasManager.RemoveCamera(currentCamera.Camera.GetCamera());
            currentCamera.Camera.Destroy();
            cameraNodes.Remove(currentCamera);
            cameraControl.RemoveCamera();
            ChangeCurrentCamera(cameraNodes.Count - 1);
        }
    }
    
    public void SetCurrentCameraPositionFromMainCamera()
    {
        if (currentCamera != null) {
            currentCamera.Coordinate = terrainManager.GetCoordinatesFromUnityPosition(sceneCamerasManager.GetPosition());
            currentCamera.Camera.SetPosition(sceneCamerasManager.GetPosition());
            currentCamera.Camera.SetEulerAngles(sceneCamerasManager.GetEulerAngles());
        }
    }
    
    public void SetMainCameraPositionFromCurrentCamera()
    {
        if (currentCamera != null) {
            sceneCamerasManager.SetPosition(currentCamera.Camera.GetPosition());
            sceneCamerasManager.SetEulerAngles(currentCamera.Camera.GetEulerAngles());
        }
    }

    public void ChangeCurrentCamera(int newCameraIndex)
    {
        if (newCameraIndex > -1) {
            if (currentCamera != null) {
                currentCamera.Camera.Show(false);
            }
            currentCamera = cameraNodes[newCameraIndex];
            currentCamera.Camera.Show(true);

            cameraParametersControl.UpdateParameters(currentCamera.Camera.GetParameters());
            cameraControl.CameraChanged(newCameraIndex);
        } else {
            currentCamera = null;
        }
    }

    public void DisplayCameraPreview(bool display)
    {
        cameraPreview.SetActive(display);

        if (currentCamera != null) {
            currentCamera.Camera.Show(display);
        }
    }

    public CameraPrefab GetCurrentCamera()
    {
        if (currentCamera == null) {
            return null;
        } else {
            return currentCamera.Camera;
        }
    }

    protected override void CreateObject(GeoJSON.Net.Feature.Feature feature, Location location)
    {
        string name = "";
        if (feature.Properties.ContainsKey("name")) {
            name = feature.Properties["name"] as string;
        }

        List<float> eulerAngles = new List<float>();
        try {
            eulerAngles = (feature.Properties["eulerAngles"] as Newtonsoft.Json.Linq.JArray).ToObject<List<float>>();
        } catch (System.Exception) {}
        if (eulerAngles.Count < 3) {
            eulerAngles = new List<float>{ 0, 0, 0 };
        }

        CameraParameters cameraParameters = new CameraParameters();
        try {
            cameraParameters = new CameraParameters((feature.Properties["parameters"] as Newtonsoft.Json.Linq.JObject).ToObject<CameraParametersSerialized>());
        } catch (System.Exception) {}

        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }

        CameraNode cameraNode = new CameraNode(
            name,
            new Coordinate(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude),
            location
        );
        CreateCamera(
            cameraNode,
            new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]),
            cameraParameters
        );
    }

    protected override void ClearActiveObjects()
    {
        foreach (CameraNode cameraNode in cameraNodes) {
            biomeAreasManager.RemoveCamera(cameraNode.Camera.GetCamera());
            cameraNode.Camera.Destroy();
        }
        cameraNodes.Clear();
        cameraControl.ClearCameras();
        currentCamera = null;
    }

    protected override List<GeoJSON.Net.Feature.Feature> GetFeaturesOfCurrentLocation()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (CameraNode cameraNode in cameraNodes) {
            if (cameraNode.Location != null) {
                GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                    cameraNode.Coordinate.latitude, cameraNode.Coordinate.longitude, cameraNode.Coordinate.altitude
                ));

                Vector3 eulerAngles = cameraNode.Camera.GetEulerAngles();
                Dictionary<string, object> properties = new Dictionary<string, object> {
                    {"type", "camera"},
                    {"location", cameraNode.Location.Name},
                    {"name", cameraNode.Name},
                    {"eulerAngles", Newtonsoft.Json.Linq.JArray.FromObject(new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z})},
                    {"parameters", Newtonsoft.Json.Linq.JObject.FromObject(cameraNode.Camera.GetParametersSerialized())}
                };
                
                features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
            }
        }

        return features;
    }

    private void CreateCamera(CameraNode cameraNode, Vector3 eulerAngles, CameraParameters cameraParameters)
    {   
        cameraControl.AddCameraToList(cameraNode.Name);
        cameraNode.Camera = Instantiate(
            cameraObject,
            terrainManager.GetUnityPositionFromCoordinates(cameraNode.Coordinate),
            Quaternion.Euler(eulerAngles),
            transform
        ).GetComponent<CameraPrefab>();
        cameraNode.Camera.SetParameters(cameraParameters);
        biomeAreasManager.AddCamera(cameraNode.Camera.GetCamera());
        
        cameraNodes.Add(cameraNode);
        ChangeCurrentCamera(cameraNodes.Count - 1);
    }
}