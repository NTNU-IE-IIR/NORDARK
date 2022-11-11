public class CameraNode: Node
{
    public CameraPrefab Camera { get; set; }

    public CameraNode(string name, Vector3d coordinates)
    {
        Name = name;
        Coordinates = coordinates;
    }
}