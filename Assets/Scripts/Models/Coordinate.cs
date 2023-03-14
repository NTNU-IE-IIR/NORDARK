public class Coordinate
{
    private const double EARTH_RADIUS = 6371000;
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double altitude { get; set; }
    
    public Coordinate()
    {
        this.latitude = 0;
        this.longitude = 0;
        this.altitude = 0;
    }

    public Coordinate(double latitude, double longitude, double altitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = altitude;
    }

    public Coordinate(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = 0;
    }

    public Coordinate(Mapbox.Utils.Vector2d latLong, double altitude)
    {
        this.latitude = latLong.x;
        this.longitude = latLong.y;
        this.altitude = altitude;
    }

    public Coordinate(GeoJSON.Net.Geometry.IPosition position)
    {
        this.latitude = position.Latitude;
        this.longitude = position.Longitude;
        this.altitude = position.Altitude == null ? 0 : (double) position.Altitude;
    }

    public static Coordinate operator +(Coordinate a, Coordinate b) {
        return new Coordinate(a.latitude + b.latitude, a.longitude + b.longitude, a.altitude + b.altitude);
    }

    public static Coordinate operator /(Coordinate a, double f) {
        return new Coordinate(a.latitude / f, a.longitude  / f, a.altitude / f);
    }

    public static double Distance(Coordinate from, Coordinate to)
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