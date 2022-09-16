public class Location
{
    public string Name { get; set; }
    public Vector2d Coordinates { get; set; }
    public double Altitude { get; set; }
    public double UnityUnitsPerLongitude { get; set; }
    public double UnityUnitsPerLatitude { get; set; }
    public double UnityUnitsPerMeters { get; set; }
    public float WorldRelativeScale { get; set; }
    public Vector2d CameraCoordinates { get; set; }
    public double CameraAltitude { get; set; }

    public Location()
    {
        Name = "";
        Coordinates = new Vector2d(0, 0);
        Altitude = 0;
        UnityUnitsPerLongitude = 0;
        UnityUnitsPerLatitude = 0;
        UnityUnitsPerMeters = 0;
        WorldRelativeScale = 0f;
        CameraCoordinates = new Vector2d(0, 0);
        CameraAltitude = 0;
    }
}