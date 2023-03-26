using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GroundTexture
{
    public string Texture;
    public List<Coordinate> Coordinates;
    public List<List<Coordinate>> HolesCoordinates;

    public GroundTexture(string texture, ReadOnlyCollection<GeoJSON.Net.Geometry.LineString> polygons)
    {
        Texture = texture;

        // See https://www.rfc-editor.org/rfc/rfc7946#section-3.1.6:
        // The "coordinates" member of a polygon MUST be an array of linear ring coordinate arrays.
        // For Polygons with more than one of these rings, the first MUST be
        // the exterior ring, and any others MUST be interior rings.  The
        // exterior ring bounds the surface, and the interior rings (if
        // present) bound holes within the surface.
        Coordinates = new List<Coordinate>();
        HolesCoordinates = new List<List<Coordinate>>();
        for (int i=0; i<polygons.Count; ++i) {
            if (i == 0) {
                Coordinates = polygons[i].Coordinates.Select(coordinate => {
                    return new Coordinate(coordinate.Latitude, coordinate.Longitude);
                }).ToList();
            } else {
                HolesCoordinates.Add(polygons[i].Coordinates.Select(coordinate => {
                    return new Coordinate(coordinate.Latitude, coordinate.Longitude);
                }).ToList());
            }
        }
    }
}
