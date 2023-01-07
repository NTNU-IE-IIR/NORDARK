using UnityEngine;

public class Tile
{
    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;
    public Transform Transform;
    public string Id;

    public Tile(Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
    {
        MeshRenderer = tile.GetComponent<MeshRenderer>();
        MeshFilter = tile.GetComponent<MeshFilter>();
        Transform = tile.transform;
        Id = tile.UnwrappedTileId.ToString().Replace('/', '-');
    }
}
