public class Vector2d
{
    public double latitude { get; set; }
    public double longitude { get; set; }

    public Vector2d(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public Vector2d(Vector3d vector)
    {
        this.latitude = vector.latitude;
        this.longitude = vector.longitude;
    }

    public Vector2d(GeoJSON.Net.Geometry.IPosition position)
    {
        this.latitude = position.Latitude;
        this.longitude = position.Longitude;
    }

    override public string ToString()
    {
        return "{latitude:" + this.latitude.ToString() + "; longitude:" + this.longitude.ToString() + "}";
    }
}