using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraManager : MonoBehaviour
{
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    public Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

    public Vector3 GetEulerAngles()
    {
        return gameObject.transform.eulerAngles;
    }

    public void SetPositionAndEulerAngles(Vector3 position, Vector3 eulerAngles)
    {
        gameObject.transform.position = position;
        gameObject.transform.eulerAngles = eulerAngles;
    }

    public void SetParameters(CameraParameters cameraParameters)
    {
        SetSensorSize(cameraParameters.SensorSize);
        SetISO(cameraParameters.ISO);
        SetShutterSpeed(cameraParameters.ShutterSpeed);
        SetFocalLength(cameraParameters.FocalLength);
        SetAperture(cameraParameters.Aperture);
        SetShift(cameraParameters.Shift);
    }

    public void SetSensorSize(Vector2 sensorSize)
    {
        mainCamera.sensorSize = sensorSize;
    }

    public void SetISO(int ISO)
    {
        mainCamera.iso = ISO;
    }

    public void SetShutterSpeed(float shutterSpeed)
    {
        mainCamera.shutterSpeed = shutterSpeed;
    }

    public void SetFocalLength(float focalLength)
    {
        mainCamera.focalLength = focalLength;
    }

    public void SetAperture(float aperture)
    {
        mainCamera.aperture = aperture;
    }

    public void SetShift(Vector2 shift)
    {
        mainCamera.lensShift = shift;
    }
}
