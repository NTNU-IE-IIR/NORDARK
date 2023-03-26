public class CameraNode
{
    public Coordinate Coordinate { get; set; }
    public string Name { get; set; }
    public CameraPrefab Camera { get; set; }
    public Location Location { get; set; }

    public CameraNode(string name, Coordinate coordinate, Location location)
    {
        Name = name;
        Coordinate = coordinate;
        Location = location;
    }
}