using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3d
{
    public double x { get; set; }
    public double y { get; set; }
    public double altitude { get; set; }
    
    public Vector3d(double x, double y, double altitude)
    {
        this.x = x;
        this.y = y;
        this.altitude = altitude;
    }

    public Vector3d(Vector2d latLong)
    {
        this.x = latLong.x;
        this.y = latLong.y;
        this.altitude = 0;
    }

    public Vector3d(Mapbox.Utils.Vector2d latLong)
    {
        this.x = latLong.x;
        this.y = latLong.y;
        this.altitude = 0;
    }

    public Vector3d(Vector2d latLong, double altitude)
    {
        this.x = latLong.x;
        this.y = latLong.y;
        this.altitude = altitude;
    }
}