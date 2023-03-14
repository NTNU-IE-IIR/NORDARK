using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GroundTexture
{
    public string Texture;
    public List<Coordinate> Coordinates;

    public GroundTexture(string texture, ReadOnlyCollection<GeoJSON.Net.Geometry.IPosition> coordinates)
    {
        Texture = texture;

        Coordinates = new List<Coordinate>();
        foreach (GeoJSON.Net.Geometry.Position coordinate in coordinates) {
            Coordinates.Add(new Coordinate(coordinate.Latitude, coordinate.Longitude));
        }
    }
}
