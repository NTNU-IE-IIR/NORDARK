using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNode: Node
{
    public CameraPrefab Camera { get; set; }

    public CameraNode(string name, Vector2d latLong, double altitude)
    {
        Name = name;
        LatLong = latLong;
        Altitude = altitude;
    }
}