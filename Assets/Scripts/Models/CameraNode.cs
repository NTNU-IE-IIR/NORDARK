using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNode: Node
{
    public Vector3 EulerAngles { get; set; }
    public CameraParameters CameraParameters { get; set; }

    public CameraNode(string name, Vector2d latLong, double altitude, Vector3 eulerAngles, CameraParameters cameraParameters)
    {
        Name = name;
        LatLong = latLong;
        Altitude = altitude;
        EulerAngles = eulerAngles;
        CameraParameters = cameraParameters;
    }
}

public class CameraParameters
{
    public Vector2 SensorSize { get; set; }
    public int ISO { get; set; }
    public float ShutterSpeed { get; set; }
    public float FocalLength { get; set; }
    public float Aperture { get; set; }
    public Vector2 Shift { get; set; }

    public CameraParameters()
    {
        SensorSize = new Vector2(36, 24);
        ISO = 200;
        ShutterSpeed = 0.005f;
        FocalLength = 13.98f;
        Aperture = 16f;
        Shift = new Vector2(0, 0);
    }

    public CameraParameters(CameraParametersSerialized cameraParametersSerialized)
    {
        if (cameraParametersSerialized.SensorSize.Count > 1) {
            SensorSize = new Vector2(cameraParametersSerialized.SensorSize[0], cameraParametersSerialized.SensorSize[1]);
        } else {
            SensorSize = new Vector2(0, 0);
        }
        ISO = cameraParametersSerialized.ISO;
        ShutterSpeed = cameraParametersSerialized.ShutterSpeed;
        FocalLength = cameraParametersSerialized.FocalLength;
        Aperture = cameraParametersSerialized.Aperture;
        if (cameraParametersSerialized.Shift.Count > 1) {
            Shift = new Vector2(cameraParametersSerialized.Shift[0], cameraParametersSerialized.Shift[1]);
        } else {
            Shift = new Vector2(0, 0);
        }
    }
}

public class CameraParametersSerialized
{
    public List<float> SensorSize { get; set; }
    public int ISO { get; set; }
    public float ShutterSpeed { get; set; }
    public float FocalLength { get; set; }
    public float Aperture { get; set; }
    public List<float> Shift { get; set; }

    public CameraParametersSerialized()
    {
    }

    public CameraParametersSerialized(CameraParameters cameraParameters)
    {
        SensorSize = new List<float>{cameraParameters.SensorSize.x, cameraParameters.SensorSize.y};
        ISO = cameraParameters.ISO;
        ShutterSpeed = cameraParameters.ShutterSpeed;
        FocalLength = cameraParameters.FocalLength;
        Aperture = cameraParameters.Aperture;
        Shift = new List<float>{cameraParameters.Shift.x, cameraParameters.Shift.y};
    }
}