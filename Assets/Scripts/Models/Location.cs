public class Location
{
    public string Name { get; set; }
    public Vector2d Coordinates { get; set; }
    public double Altitude { get; set; }
    public Vector2d CameraCoordinates { get; set; }
    public double CameraAltitude { get; set; }

    public Location()
    {
        Name = "";
        Coordinates = new Vector2d(0, 0);
        Altitude = 0;
        CameraCoordinates = new Vector2d(0, 0);
        CameraAltitude = 0;
    }
}