using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CamerasManager : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private MainCameraManager mainCameraManager;
    [SerializeField]
    private CameraControl cameraControl;
    [SerializeField]
    private CameraParametersControl cameraParametersControl;

    private List<CameraNode> cameras;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(mainCameraManager);
        Assert.IsNotNull(cameraControl);
        Assert.IsNotNull(cameraParametersControl);

        cameras = new List<CameraNode>();
    }

    public void CreateCameras(List<CameraNodeSerialized> cameraNodesSerialized)
    {
        foreach (CameraNodeSerialized cameraNodeSerialized in cameraNodesSerialized) {
            CreateCamera(new CameraNode(cameraNodeSerialized));
        }
    }

    public void CreateDefaultCamera()
    {
        CameraParameters cameraParameters = new CameraParameters();
        cameraParametersControl.UpdateParameters(cameraParameters);
        CreateCamera(new CameraNode("New Camera", mainCameraManager.GetPosition(), mainCameraManager.GetEulerAngles(), cameraParameters));
    }

    public void ClearCameras()
    {
        cameras.Clear();
        cameraControl.ClearCameras();
    }

    public (Mapbox.Utils.Vector2d, Dictionary<string, object>) GetCameraFeature()
    {
        (Mapbox.Utils.Vector2d, Dictionary<string, object>) feature;
        
        feature.Item1 = new Mapbox.Utils.Vector2d(0, 0);

        feature.Item2 = new Dictionary<string, object>();
        Mapbox.Utils.Vector2d location = mapManager.GetMapLocation();
        feature.Item2.Add("mapCenter", new List<double>{location.x, location.y});
        feature.Item2.Add("cameraPos", new List<float>{mainCameraManager.GetPosition().x, mainCameraManager.GetPosition().y, mainCameraManager.GetPosition().z});
        feature.Item2.Add("cameraAngles", new List<float>{mainCameraManager.GetEulerAngles().x, mainCameraManager.GetEulerAngles().y, mainCameraManager.GetEulerAngles().z});


        List<CameraNodeSerialized> cameraNodesSerialized = new List<CameraNodeSerialized>();
        foreach (CameraNode camera in cameras) {
            cameraNodesSerialized.Add(new CameraNodeSerialized(camera));
        }
        feature.Item2.Add("cameras", cameraNodesSerialized);

        return feature;
    }

    public void ResetCameraPosition(int cameraIndex)
    {
        SetCameraPosition(cameraIndex, true);
    }
    
    public void UpdateCameraPosition(int cameraIndex)
    {
        SetCameraPosition(cameraIndex, false);
    }

    public void AddCamera(string name)
    {
        CreateCamera(new CameraNode(name, mainCameraManager.GetPosition(), mainCameraManager.GetEulerAngles(), cameraParametersControl.GetCameraParameters()));
    }

    public void DeleteCamera(string name)
    {
        cameras.RemoveAll(camera => camera.Name == name);
        CameraChanged(cameras.Count - 1);
    }

    public void CameraChanged(int cameraIndex)
    {
        if (cameraIndex > -1) {
            mainCameraManager.SetParameters(cameras[cameraIndex].CameraParameters);
            cameraParametersControl.UpdateParameters(cameras[cameraIndex].CameraParameters);
            cameraControl.CameraChanged(cameraIndex);
            ResetCameraPosition(cameraIndex);
        }
    }

    public void SetSensorSize(Vector2 sensorSize)
    {
        mainCameraManager.SetSensorSize(sensorSize);

        int cameraIndex = cameraControl.GetCameraIndex();
        if (cameraIndex > -1) {
            cameras[cameraIndex].CameraParameters.SensorSize = sensorSize;
        }
    }

    public void SetISO(int ISO)
    {
        mainCameraManager.SetISO(ISO);

        int cameraIndex = cameraControl.GetCameraIndex();
        if (cameraIndex > -1) {
            cameras[cameraIndex].CameraParameters.ISO = ISO;
        }
    }

    public void SetShutterSpeed(float shutterSpeed)
    {
        mainCameraManager.SetShutterSpeed(shutterSpeed);

        int cameraIndex = cameraControl.GetCameraIndex();
        if (cameraIndex > -1) {
            cameras[cameraIndex].CameraParameters.ShutterSpeed = shutterSpeed;
        }
    }

    public void SetFocalLength(float focalLength)
    {
        mainCameraManager.SetFocalLength(focalLength);

        int cameraIndex = cameraControl.GetCameraIndex();
        if (cameraIndex > -1) {
            cameras[cameraIndex].CameraParameters.FocalLength = focalLength;
        }
    }

    public void SetAperture(float aperture)
    {
        mainCameraManager.SetAperture(aperture);

        int cameraIndex = cameraControl.GetCameraIndex();
        if (cameraIndex > -1) {
            cameras[cameraIndex].CameraParameters.Aperture = aperture;
        }
    }

    public void SetShift(Vector2 shift)
    {
        mainCameraManager.SetShift(shift);

        int cameraIndex = cameraControl.GetCameraIndex();
        if (cameraIndex > -1) {
            cameras[cameraIndex].CameraParameters.Shift = shift;
        }
    }

    private void SetCameraPosition(int cameraIndex, bool reset)
    {
        if (cameraIndex < cameras.Count) {
            if (reset) {
                mainCameraManager.SetPositionAndEulerAngles(cameras[cameraIndex].Position, cameras[cameraIndex].EulerAngles);
            } else {
                cameras[cameraIndex].Position = mainCameraManager.GetPosition();
                cameras[cameraIndex].EulerAngles = mainCameraManager.GetEulerAngles();
            }
        }
    }

    private void CreateCamera(CameraNode camera)
    {
        cameras.Add(camera);
        cameraControl.AddCameraToList(camera.Name);
        CameraChanged(cameras.Count - 1);
    }
}