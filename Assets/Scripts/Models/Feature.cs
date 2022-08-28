using System.Collections;
using System.Collections.Generic;

public class Feature
{
    public Vector3d Coordinates { get; set; }
    public Dictionary<string, object> Properties { get; set; }

    public Feature()
    {
        Coordinates = new Vector3d(0, 0, 0);
        Properties = new Dictionary<string, object>();
    }
}