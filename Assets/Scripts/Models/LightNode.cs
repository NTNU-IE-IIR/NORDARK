using UnityEngine;

public class LightNode: Node
{
    public Vector2 LatLong { get; set; }
    public GameObject Light { get; set; }
    public IESLight IESLight { get; set; }
    public string PrefabName { get; set; }
}