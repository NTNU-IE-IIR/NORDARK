public class Location
{
    public string Name { get; set; }
    public Vector3d Coordinates { get; set; }
    public Vector3d CameraCoordinates { get; set; }
    public UnityEngine.Vector3 CameraAngles { get; set; }

    public Location()
    {
        Name = "";
        Coordinates = new Vector3d();
        CameraCoordinates = new Vector3d();
        CameraAngles = new UnityEngine.Vector3();
    }
}