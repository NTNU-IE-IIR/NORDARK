using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CamerasManager : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private CameraControl cameraControl;
    [SerializeField]
    private CameraParametersControl cameraParametersControl;
    [SerializeField]
    private GameObject cameraPreview;
    [SerializeField]
    private GameObject cameraObject;
    [SerializeField]
    private GameObject mainCamera;

    private List<CameraNode> cameras;
    private CameraNode currentCamera;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(cameraControl);
        Assert.IsNotNull(cameraParametersControl);
        Assert.IsNotNull(cameraPreview);
        Assert.IsNotNull(cameraObject);
        Assert.IsNotNull(mainCamera);

        cameras = new List<CameraNode>();
        currentCamera = null;
    }

    public void CreateCamera()
    {
        CreateCamera(
            new CameraNode(
                DetermineNewCameraName(),
                mapManager.GetCoordinatesFromUnityPosition(mainCamera.transform.position),
                mapManager.GetAltitudeFromUnityPosition(mainCamera.transform.position)
            ),
            mainCamera.transform.eulerAngles,
            cameraParametersControl.GetCameraParameters()
        );
    }

    public void CreateCamera(CameraNode camera, Vector3 eulerAngles, CameraParameters cameraParameters)
    {
        cameraControl.AddCameraToList(camera.Name);
        camera.Camera = (Instantiate(
            cameraObject,
            mapManager.GetUnityPositionFromCoordinatesAndAltitude(camera.LatLong, camera.Altitude),
            Quaternion.Euler(eulerAngles),
            transform
        ) as GameObject).GetComponent<CameraPrefab>();
        camera.Camera.SetParameters(cameraParameters);
        
        cameras.Add(camera);
        ChangeCurrentCamera(cameras.Count - 1);
    }

    public void ClearCameras()
    {
        foreach (CameraNode camera in cameras) {
            camera.Camera.Destroy();
        }
        cameras.Clear();
        cameraControl.ClearCameras();
        currentCamera = null;
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
            feature.Coordinates = new Vector3d(camera.LatLong, camera.Altitude);
            features.Add(feature);
        }
        return features;
    }
    
    public void SetCurrentCameraPositionFromMainCamera()
    {
        if (currentCamera != null) {
            currentCamera.LatLong = mapManager.GetCoordinatesFromUnityPosition(mainCamera.transform.position);
            currentCamera.Altitude = mapManager.GetAltitudeFromUnityPosition(mainCamera.transform.position);
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

    public void UpdateCamerasPosition()
    {
        foreach (CameraNode camera in cameras) {
            camera.Camera.SetPosition(mapManager.GetUnityPositionFromCoordinatesAndAltitude(camera.LatLong, camera.Altitude));
        }
    }

    public void SetMainCameraPosition(Vector2d latLong, double altitude)
    {
        mainCamera.transform.position = mapManager.GetUnityPositionFromCoordinatesAndAltitude(latLong, altitude);
    }

    public void SetMainCameraAngles(Vector3 angles)
    {
        mainCamera.transform.eulerAngles = angles;
    }

    private string DetermineNewCameraName(int index = 1)
    {
        foreach (CameraNode camera in cameras) {
            if (camera.Name == "Camera " + index.ToString()) {
                return DetermineNewCameraName(index + 1);
            }
        }
        return "Camera " + index.ToString();
    }
}