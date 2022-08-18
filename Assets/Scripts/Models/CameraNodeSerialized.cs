using System.Collections;
using System.Collections.Generic;

public class CameraNodeSerialized: Node
{
    public List<float> Position { get; private set; }
    public List<float> EulerAngles { get; private set; }
    public CameraParametersSerialized Parameters { get; set; }

    public CameraNodeSerialized()
    {
        Name = "";
        Position = new List<float>();
        EulerAngles = new List<float>();
        Parameters = new CameraParametersSerialized();
    }

    public CameraNodeSerialized(CameraNode cameraNode)
    {
        Name = cameraNode.Name;
        Position = new List<float>{cameraNode.Position.x, cameraNode.Position.y, cameraNode.Position.z};
        EulerAngles = new List<float>{cameraNode.EulerAngles.x, cameraNode.EulerAngles.y, cameraNode.EulerAngles.z};
        Parameters = new CameraParametersSerialized(cameraNode.CameraParameters);
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