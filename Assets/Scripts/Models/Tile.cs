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

    public (Vector3, Vector3) GetBoundaries()
    {
        return (new Vector3(
            Transform.position.x - MeshFilter.mesh.bounds.size.x / 2,
            Transform.position.y,
            Transform.position.z - MeshFilter.mesh.bounds.size.z / 2
        ), new Vector3(
            Transform.position.x + MeshFilter.mesh.bounds.size.x / 2,
            Transform.position.y,
            Transform.position.z + MeshFilter.mesh.bounds.size.z / 2
        ));
    }
}
