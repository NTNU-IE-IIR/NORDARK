public class Location
{
    public string Name { get; set; }
    public Coordinate Coordinate { get; set; }
    public Coordinate CameraCoordinates { get; set; }
    public UnityEngine.Vector3 CameraAngles { get; set; }

    public Location()
    {
        Name = "";
        Coordinate = new Coordinate();
        CameraCoordinates = new Coordinate();
        CameraAngles = new UnityEngine.Vector3();
    }
}