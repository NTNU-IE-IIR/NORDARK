public class VegetationObject
{
    public Coordinate Coordinate { get; set; }
    public VegetationObjectPrefab GameObject { get; set; }
    public string PrefabName { get; set; }
    public Location Location { get; set; }

    public VegetationObject(Location location)
    {
        Coordinate = new Coordinate();
        PrefabName = "";
        Location = location;
    }

    public VegetationObject(Coordinate coordinate, Location location): this(location)
    {
        Coordinate = coordinate;
    }
}