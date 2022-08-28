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

    public Vector2d(Mapbox.Utils.Vector2d vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }

    public Vector2d(List<double> list)
    {
        if (list.Count > 1) {
            this.x = list[0];
            this.y = list[1];
        }
    }
}