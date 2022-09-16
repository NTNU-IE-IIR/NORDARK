using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode: Node
{
    public GameObject Tree { get; set; }

    public TreeNode(Vector2d latLong, double altitude)
    {
        LatLong = latLong;
        Altitude = altitude;
    }
}