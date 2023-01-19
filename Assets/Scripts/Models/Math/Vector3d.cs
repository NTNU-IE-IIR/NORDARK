public class Vector3d
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double altitude { get; set; }
    
    public Vector3d()
    {
        this.latitude = 0;
        this.longitude = 0;
        this.altitude = 0;
    }

    public Vector3d(double latitude, double longitude, double altitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = altitude;
    }

    public Vector3d(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = 0;
    }

    public Vector3d(Vector2d latLong, double altitude = 0)
    {
        this.latitude = latLong.latitude;
        this.longitude = latLong.longitude;
        this.altitude = altitude;
    }

    public Vector3d(Mapbox.Utils.Vector2d latLong, double altitude)
    {
        this.latitude = latLong.x;
        this.longitude = latLong.y;
        this.altitude = altitude;
    }

    public Vector3d(GeoJSON.Net.Geometry.IPosition position)
    {
        this.latitude = position.Latitude;
        this.longitude = position.Longitude;
        this.altitude = position.Altitude == null ? 0 : (double) position.Altitude;
    }

    override public string ToString()
    {
        return "{latitude:" + latitude.ToString() + "; longitude:" + longitude.ToString() + "; altitude:" + altitude.ToString() + "}";
    }
}