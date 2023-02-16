public class Vector3d
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double altitude { get; set; }
    private const double EARTH_RADIUS = 6371000;
    
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

    public static double Distance(Vector3d from, Vector3d to)
    {
        // Haversine formula
        double fromLatRad = from.latitude * System.Math.PI/180;
        double toLatRad = to.latitude * System.Math.PI/180;
        double deltaLat = toLatRad-fromLatRad;
        double deltaLon = (to.longitude-from.longitude) * System.Math.PI/180;

        double a =
            System.Math.Sin(deltaLat/2) * System.Math.Sin(deltaLat/2) +
            System.Math.Cos(fromLatRad) * System.Math.Cos(toLatRad) *
            System.Math.Sin(deltaLon/2) * System.Math.Sin(deltaLon/2);

        return EARTH_RADIUS * 2 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1-a));
    }

    override public string ToString()
    {
        return "{latitude:" + latitude.ToString() + "; longitude:" + longitude.ToString() + "; altitude:" + altitude.ToString() + "}";
    }
}