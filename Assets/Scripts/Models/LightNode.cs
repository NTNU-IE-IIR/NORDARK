using UnityEngine;

public class LightNode: Node
{
    public LightPrefab Light { get; set; }
    public string PrefabName { get; set; }

    public LightNode()
    {
        LatLong = new Vector2d(0, 0);
        Altitude = 0;
        Name = "";
        PrefabName = "";
    }

    public LightNode(Vector2d latLong, double altitude)
    {
        LatLong = latLong;
        Altitude = altitude;
    }
}