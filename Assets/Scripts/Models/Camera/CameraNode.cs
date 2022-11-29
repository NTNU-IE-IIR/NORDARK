public class CameraNode
{
    public Vector3d Coordinates { get; set; }
    public string Name { get; set; }
    public CameraPrefab Camera { get; set; }

    public CameraNode(string name, Vector3d coordinates)
    {
        Name = name;
        Coordinates = coordinates;
    }
}