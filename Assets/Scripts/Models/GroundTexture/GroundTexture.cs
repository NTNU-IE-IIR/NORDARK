using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GroundTexture
{
    public string Texture;
    public List<Vector3d> Coordinates;

    public GroundTexture(string texture, ReadOnlyCollection<GeoJSON.Net.Geometry.IPosition> coordinates)
    {
        Texture = texture;

        Coordinates = new List<Vector3d>();
        foreach (GeoJSON.Net.Geometry.Position coordinate in coordinates) {
            Coordinates.Add(new Vector3d(coordinate.Latitude, coordinate.Longitude));
        }
    }
}
