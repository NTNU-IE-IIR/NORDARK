public class Location
{
    public enum TerrainType
    {
        Map,
        Basic
    }
    public string Name { get; set; }
    public Coordinate Coordinate { get; set; }
    public int Zoom { get; set; }
    public Coordinate CameraCoordinates { get; set; }
    public UnityEngine.Vector3 CameraAngles { get; set; }
    public TerrainType Type;

    public Location()
    {
        Name = "";
        Coordinate = new Coordinate();
        Zoom = 0;
        CameraCoordinates = new Coordinate();
        CameraAngles = new UnityEngine.Vector3();
        Type = TerrainType.Map;
    }
}