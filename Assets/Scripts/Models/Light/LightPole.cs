public class LightPole
{
    public Vector3d Coordinates { get; set; }
    public string Name { get; set; }
    public LightPrefab Light { get; set; }
    public string PrefabName { get; set; }

    public LightPole()
    {
        Coordinates = new Vector3d();
        Name = "";
        PrefabName = "";
    }

    public LightPole(Vector3d coordinates): base()
    {
        Coordinates = coordinates;
    }
}