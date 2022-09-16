using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPrefab : MonoBehaviour
{
    private Camera cameraComponent;

    void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        Show(false);
    }

    public void SetPosition(Vector3 position)
    {
        gameObject.transform.position = position;
    }

    public Vector3 GetEulerAngles()
    {
        return gameObject.transform.eulerAngles;
    }

    public void SetEulerAngles(Vector3 eulerAngles)
    {
        gameObject.transform.eulerAngles = eulerAngles;
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
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

    public CameraParametersSerialized GetParametersSerialized()
    {
        CameraParametersSerialized cameraParametersSerialized = new CameraParametersSerialized();
        cameraParametersSerialized.SensorSize = new List<float>{cameraComponent.sensorSize.x, cameraComponent.sensorSize.y};
        cameraParametersSerialized.ISO = cameraComponent.iso;
        cameraParametersSerialized.ShutterSpeed = cameraComponent.shutterSpeed;
        cameraParametersSerialized.FocalLength = cameraComponent.focalLength;
        cameraParametersSerialized.Aperture = cameraComponent.aperture;
        cameraParametersSerialized.Shift = new List<float>{cameraComponent.lensShift.x, cameraComponent.lensShift.y};
        return cameraParametersSerialized;
    }

    public CameraParameters GetParameters()
    {
        return new CameraParameters(GetParametersSerialized());
    }

    public void SetSensorSize(Vector2 sensorSize)
    {
        cameraComponent.sensorSize = sensorSize;
    }

    public void SetISO(int ISO)
    {
        cameraComponent.iso = ISO;
    }

    public void SetShutterSpeed(float shutterSpeed)
    {
        cameraComponent.shutterSpeed = shutterSpeed;
    }

    public void SetFocalLength(float focalLength)
    {
        cameraComponent.focalLength = focalLength;
    }

    public void SetAperture(float aperture)
    {
        cameraComponent.aperture = aperture;
    }

    public void SetShift(Vector2 shift)
    {
        cameraComponent.lensShift = shift;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
