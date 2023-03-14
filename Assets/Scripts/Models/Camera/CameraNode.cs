public class CameraNode
{
    public Coordinate Coordinate { get; set; }
    public string Name { get; set; }
    public CameraPrefab Camera { get; set; }

    public CameraNode(string name, Coordinate coordinate)
    {
        Name = name;
        Coordinate = coordinate;
    }
}