using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2d
{
    public double x { get; set; }
    public double y { get; set; }

    public Vector2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2d(Vector3d vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }

    public Vector2d(GeoJSON.Net.Geometry.IPosition position)
    {
        this.x = position.Latitude;
        this.y = position.Longitude;
    }

    override public string ToString()
    {
        return "{x:" + x.ToString() + "; y:" + y.ToString() + "}";
    }
}