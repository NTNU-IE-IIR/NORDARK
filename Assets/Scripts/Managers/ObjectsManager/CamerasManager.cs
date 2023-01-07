using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class CamerasManager : MonoBehaviour, IObjectsManager
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private CameraParametersControl cameraParametersControl;
    [SerializeField] private GameObject cameraPreview;
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private GameObject mainCamera;
    private List<CameraNode> cameras;
    private CameraNode currentCamera;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(cameraControl);
        Assert.IsNotNull(cameraParametersControl);
        Assert.IsNotNull(cameraPreview);
        Assert.IsNotNull(cameraObject);
        Assert.IsNotNull(mainCamera);

        cameras = new List<CameraNode>();
        currentCamera = null;
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        string name = "";
        if (feature.Properties.ContainsKey("name")) {
            name = feature.Properties["name"] as string;
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

        CameraParameters cameraParameters = new CameraParameters();
        try
        {
            cameraParameters = new CameraParameters((feature.Properties["parameters"] as Newtonsoft.Json.Linq.JObject).ToObject<CameraParametersSerialized>());
        }
        catch (System.Exception)
        {}

        GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
        double altitude = 0;
        if (point.Coordinates.Altitude != null) {
            altitude = (double) point.Coordinates.Altitude;
        }

        CameraNode cameraNode = new CameraNode(
            name,
            new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude, altitude)
        );
        CreateCamera(
            cameraNode,
            new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]),
            cameraParameters
        );
    }

    public void Clear()
    {
        foreach (CameraNode camera in cameras) {
            camera.Camera.Destroy();
        }
        cameras.Clear();
        cameraControl.ClearCameras();
        currentCamera = null;
    }

    public void OnLocationChanged()
    {
        foreach (CameraNode camera in cameras) {
            camera.Camera.SetPosition(mapManager.GetUnityPositionFromCoordinates(camera.Coordinates));
        }
        Location currentLocation = locationsManager.GetCurrentLocation();
        if (currentLocation != null) {
            mainCamera.transform.position = mapManager.GetUnityPositionFromCoordinates(currentLocation.CameraCoordinates);
            mainCamera.transform.eulerAngles = currentLocation.CameraAngles;
        }
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (CameraNode camera in cameras) {
            GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                camera.Coordinates.x, camera.Coordinates.y, camera.Coordinates.altitude
            ));

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("type", "camera");
            properties.Add("name", camera.Name);
            Vector3 eulerAngles = camera.Camera.GetEulerAngles();
            properties.Add("eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z});
            properties.Add("parameters", camera.Camera.GetParametersSerialized());
            
            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
        }

        return features;
    }

    public void CreateCamera()
    {
        CreateCamera(
            new CameraNode(
                Utils.DetermineNewName(cameras.Select(camera => camera.Name).ToList(), "Camera"),
                mapManager.GetCoordinatesFromUnityPosition(mainCamera.transform.position)
            ),
            mainCamera.transform.eulerAngles,
            cameraParametersControl.GetCameraParameters()
        );
    }

    public void DeleteCamera()
    {
        if (currentCamera != null) {
            currentCamera.Camera.Destroy();
            cameras.Remove(currentCamera);
            cameraControl.RemoveCamera();
            ChangeCurrentCamera(cameras.Count - 1);
        }
    }
    
    public void SetCurrentCameraPositionFromMainCamera()
    {
        if (currentCamera != null) {
            currentCamera.Coordinates = mapManager.GetCoordinatesFromUnityPosition(mainCamera.transform.position);
            currentCamera.Camera.SetPosition(mainCamera.transform.position);
            currentCamera.Camera.SetEulerAngles(mainCamera.transform.eulerAngles);
        }
    }
    
    public void SetMainCameraPositionFromCurrentCamera()
    {
        if (currentCamera != null) {
            mainCamera.transform.position = currentCamera.Camera.GetPosition();
            mainCamera.transform.eulerAngles = currentCamera.Camera.GetEulerAngles();
        }
    }

    public void ChangeCurrentCamera(int newCameraIndex)
    {
        if (newCameraIndex > -1) {
            if (currentCamera != null) {
                currentCamera.Camera.Show(false);
            }
            currentCamera = cameras[newCameraIndex];
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

    private void CreateCamera(CameraNode camera, Vector3 eulerAngles, CameraParameters cameraParameters)
    {
        cameraControl.AddCameraToList(camera.Name);
        camera.Camera = (Instantiate(
            cameraObject,
            mapManager.GetUnityPositionFromCoordinates(camera.Coordinates),
            Quaternion.Euler(eulerAngles),
            transform
        ) as GameObject).GetComponent<CameraPrefab>();
        camera.Camera.SetParameters(cameraParameters);
        
        cameras.Add(camera);
        ChangeCurrentCamera(cameras.Count - 1);
    }
}