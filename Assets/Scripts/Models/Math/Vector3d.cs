using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3d
{
    public double x { get; set; }
    public double y { get; set; }
    public double altitude { get; set; }
    
    public Vector3d()
    {
        this.x = 0;
        this.y = 0;
        this.altitude = 0;
    }

    public Vector3d(double x, double y, double altitude)
    {
        this.x = x;
        this.y = y;
        this.altitude = altitude;
    }

    public Vector3d(Vector2d latLong, double altitude = 0)
    {
        this.x = latLong.x;
        this.y = latLong.y;
        this.altitude = altitude;
    }

    public Vector3d(Mapbox.Utils.Vector2d latLong, double altitude)
    {
        this.x = latLong.x;
        this.y = latLong.y;
        this.altitude = altitude;
    }

    public Vector3d(GeoJSON.Net.Geometry.IPosition position)
    {
        this.x = position.Latitude;
        this.y = position.Longitude;
        this.altitude = position.Altitude == null ? 0 : (double) position.Altitude;
    }

    override public string ToString()
    {
        return "{x:" + x.ToString() + "; y:" + y.ToString() + "; altitude:" + altitude.ToString() + "}";
    }
}