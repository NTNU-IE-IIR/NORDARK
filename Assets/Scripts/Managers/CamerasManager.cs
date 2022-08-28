using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CamerasManager : MonoBehaviour
{
    [SerializeField]
    private EnvironmentManager environmentManager;
    [SerializeField]
    private MainCameraManager mainCameraManager;
    [SerializeField]
    private CameraControl cameraControl;
    [SerializeField]
    private CameraParametersControl cameraParametersControl;

    private List<CameraNode> cameras;
    private int currentCameraIndex;

    void Awake()
    {
        Assert.IsNotNull(environmentManager);
        Assert.IsNotNull(mainCameraManager);
        Assert.IsNotNull(cameraControl);
        Assert.IsNotNull(cameraParametersControl);

        cameras = new List<CameraNode>();
        currentCameraIndex = -1;
    }

    public void AddCamera(string name)
    {
        CreateCamera(new CameraNode(
            name,
            environmentManager.GetCoordinatesFromUnityPosition(mainCameraManager.GetPosition()),
            environmentManager.GetAltitudeFromUnityPosition(mainCameraManager.GetPosition()),
            mainCameraManager.GetEulerAngles(),
            cameraParametersControl.GetCameraParameters()
        ));
    }

    public void CreateCamera(CameraNode camera)
    {
        cameras.Add(camera);
        cameraControl.AddCameraToList(camera.Name);
        ChangeCurrentCamera(cameras.Count - 1);
    }

    public void CreateDefaultCamera()
    {
        CameraParameters cameraParameters = new CameraParameters();
        cameraParametersControl.UpdateParameters(cameraParameters);
        CreateCamera(new CameraNode(
            "New Camera",
            environmentManager.GetCoordinatesFromUnityPosition(mainCameraManager.GetPosition()),
            environmentManager.GetAltitudeFromUnityPosition(mainCameraManager.GetPosition()),
            mainCameraManager.GetEulerAngles(),
            cameraParameters
        ));
    }

    public void ClearCameras()
    {
        cameras.Clear();
        cameraControl.ClearCameras();
    }

    public void DeleteCamera(int cameraIndex)
    {
        cameras.RemoveAt(cameraIndex);
        ChangeCurrentCamera(cameras.Count - 1);
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (CameraNode camera in cameras) {
            Feature feature = new Feature();
            feature.Properties.Add("type", "camera");
            feature.Properties.Add("name", camera.Name);
            feature.Properties.Add("eulerAngles", new List<float>{camera.EulerAngles.x, camera.EulerAngles.y, camera.EulerAngles.z});
            feature.Properties.Add("parameters", new CameraParametersSerialized(camera.CameraParameters));
            feature.Coordinates = new Vector3d(camera.LatLong, camera.Altitude);
            features.Add(feature);
        }
        return features;
    }

    public void ResetMainCameraPosition()
    {
        if (currentCameraIndex > -1) {
            Vector3 position = environmentManager.GetUnityPositionFromCoordinatesAndAltitude(cameras[currentCameraIndex].LatLong, cameras[currentCameraIndex].Altitude);
            mainCameraManager.SetPosition(position);
            mainCameraManager.SetEulerAngles(cameras[currentCameraIndex].EulerAngles);
        }
    }
    
    public void UpdateCameraPosition()
    {
        cameras[currentCameraIndex].LatLong = environmentManager.GetCoordinatesFromUnityPosition(mainCameraManager.GetPosition());
        cameras[currentCameraIndex].Altitude = environmentManager.GetAltitudeFromUnityPosition(mainCameraManager.GetPosition());
        cameras[currentCameraIndex].EulerAngles = mainCameraManager.GetEulerAngles();
    }

    public void ChangeCurrentCamera(int newCameraIndex)
    {
        if (newCameraIndex > -1 && newCameraIndex < cameras.Count) {
            currentCameraIndex = newCameraIndex;
            mainCameraManager.SetParameters(cameras[newCameraIndex].CameraParameters);
            cameraParametersControl.UpdateParameters(cameras[newCameraIndex].CameraParameters);
            cameraControl.CameraChanged(newCameraIndex);
            ResetMainCameraPosition();
        }
    }

    public void SetSensorSize(Vector2 sensorSize)
    {
        mainCameraManager.SetSensorSize(sensorSize);
        cameras[currentCameraIndex].CameraParameters.SensorSize = sensorSize;
    }

    public void SetISO(int ISO)
    {
        mainCameraManager.SetISO(ISO);
        cameras[currentCameraIndex].CameraParameters.ISO = ISO;
    }

    public void SetShutterSpeed(float shutterSpeed)
    {
        mainCameraManager.SetShutterSpeed(shutterSpeed);
        cameras[currentCameraIndex].CameraParameters.ShutterSpeed = shutterSpeed;
    }

    public void SetFocalLength(float focalLength)
    {
        mainCameraManager.SetFocalLength(focalLength);
        cameras[currentCameraIndex].CameraParameters.FocalLength = focalLength;
    }

    public void SetAperture(float aperture)
    {
        mainCameraManager.SetAperture(aperture);
        cameras[currentCameraIndex].CameraParameters.Aperture = aperture;
    }

    public void SetShift(Vector2 shift)
    {
        mainCameraManager.SetShift(shift);
        cameras[currentCameraIndex].CameraParameters.Shift = shift;
    }
}