using System.Collections.Generic;

public class Feature
{
    public List<Vector3d> Coordinates { get; set; }
    public Dictionary<string, object> Properties { get; set; }

    public Feature()
    {
        Coordinates = new List<Vector3d>();
        Properties = new Dictionary<string, object>();
    }
}