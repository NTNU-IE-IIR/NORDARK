using UnityEngine;

public class CameraNode: Node
{
    public Vector3 Position { get; set; }
    public Vector3 EulerAngles { get; set; }
    public CameraParameters CameraParameters { get; set; }

    public CameraNode(string name, Vector3 position, Vector3 eulerAngles, CameraParameters cameraParameters)
    {
        Name = name;
        Position = position;
        EulerAngles = eulerAngles;
        CameraParameters = cameraParameters;
    }

    public CameraNode(CameraNodeSerialized cameraNodeSerialized)
    {
        Name = cameraNodeSerialized.Name;
        if (cameraNodeSerialized.Position.Count > 2) {
            Position = new Vector3(cameraNodeSerialized.Position[0], cameraNodeSerialized.Position[1], cameraNodeSerialized.Position[2]);
        } else {
            Position = new Vector3(0, 0, 0);
        }
        if (cameraNodeSerialized.EulerAngles.Count > 2) {
            EulerAngles = new Vector3(cameraNodeSerialized.EulerAngles[0], cameraNodeSerialized.EulerAngles[1], cameraNodeSerialized.EulerAngles[2]);
        } else {
            EulerAngles = new Vector3(0, 0, 0);
        }
        CameraParameters = new CameraParameters(cameraNodeSerialized.Parameters);
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