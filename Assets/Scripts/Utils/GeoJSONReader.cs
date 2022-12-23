using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GeoJSONReader
{
    public static GeoJSON.Net.Feature.FeatureCollection ReadFile(string filename)
    {
        StreamReader rd = new StreamReader(filename);
        GeoJSON.Net.Feature.FeatureCollection featureCollection = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(rd.ReadToEnd());
        rd.Close();
        return featureCollection;
    }

    public static void SaveToGeojson(string filename, List<Feature> features)
    {
        GeoJSON.Net.Feature.FeatureCollection featureCollection = new GeoJSON.Net.Feature.FeatureCollection();

        for (int i = 0; i < features.Count; i++) {
            GeoJSON.Net.Geometry.IGeometryObject geometry;
            
            if (features[i].Coordinates.Count > 1) {
                List<List<List<double>>> coordinates = new List<List<List<double>>> {new List<List<double>>()};
                foreach(Vector3d coordinate in features[i].Coordinates) {
                    coordinates[0].Add(new List<double>{coordinate.x, coordinate.y, coordinate.altitude});
                }
                geometry = new GeoJSON.Net.Geometry.Polygon(coordinates);
            } else {
                geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(features[i].Coordinates[0].x, features[i].Coordinates[0].y, features[i].Coordinates[0].altitude));
            }

            GeoJSON.Net.Feature.Feature feature = new GeoJSON.Net.Feature.Feature(geometry, features[i].Properties, i.ToString());
            featureCollection.Features.Add(feature);
        }

        var json = JsonConvert.SerializeObject(featureCollection);

        json = json.Replace("\"type\":8", "\"type\":\"FeatureCollection\"");
        json = json.Replace("\"type\":7", "\"type\":\"Feature\"");
        json = json.Replace("\"type\":5", "\"type\":\"MultiPolygon\"");
        json = json.Replace("\"type\":4", "\"type\":\"Polygon\"");
        json = json.Replace("\"type\":3", "\"type\":\"MultiLineString\"");
        json = json.Replace("\"type\":0", "\"type\":\"Point\"");

        StreamWriter sw = new StreamWriter(filename);
        sw.WriteLine(json);
        sw.Close();
    }
}
