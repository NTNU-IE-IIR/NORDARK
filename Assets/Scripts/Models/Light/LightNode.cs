public class LightNode: Node
{
    public LightPrefab Light { get; set; }
    public string PrefabName { get; set; }

    public LightNode()
    {
        Coordinates = new Vector3d();
        Name = "";
        PrefabName = "";
    }

    public LightNode(Vector3d coordinates)
    {
        Coordinates = coordinates;
    }
}