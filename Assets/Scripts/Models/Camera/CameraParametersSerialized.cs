using System.Collections.Generic;

public class CameraParametersSerialized
{
    public List<float> SensorSize { get; set; }
    public int ISO { get; set; }
    public float ShutterSpeed { get; set; }
    public float FocalLength { get; set; }
    public float Aperture { get; set; }
    public List<float> Shift { get; set; }

    public CameraParametersSerialized()
    {}
}