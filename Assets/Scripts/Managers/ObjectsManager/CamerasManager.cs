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

    public void Create(Feature feature)
    {
        CameraNode cameraNode = new CameraNode(
            feature.Properties["name"] as string,
            new Vector3d(feature.Coordinates[0].x, feature.Coordinates[0].y, feature.Coordinates[0].altitude)
        );
        List<float> eulerAngles = feature.Properties["eulerAngles"] as List<float>;

        CreateCamera(
            cameraNode,
            new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]),
            feature.Properties["parameters"] as CameraParameters
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

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (CameraNode camera in cameras) {
            Feature feature = new Feature();
            feature.Properties.Add("type", "camera");
            feature.Properties.Add("name", camera.Name);
            Vector3 eulerAngles = camera.Camera.GetEulerAngles();
            feature.Properties.Add("eulerAngles", new List<float>{eulerAngles.x, eulerAngles.y, eulerAngles.z});
            feature.Properties.Add("parameters", camera.Camera.GetParametersSerialized());
            feature.Coordinates = new List<Vector3d> {camera.Coordinates};
            features.Add(feature);
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