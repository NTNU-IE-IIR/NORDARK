using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class GeoJSONParser
{
    public static GeoJSON.Net.Feature.FeatureCollection FileToFeatureCollection(string filename)
    {
        // This function can throw an exception if the file is already opened by another application
        StreamReader rd = new StreamReader(filename);
        GeoJSON.Net.Feature.FeatureCollection featureCollection = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(rd.ReadToEnd());
        rd.Close();
        return featureCollection;
    }

    public static GeoJSON.Net.Feature.FeatureCollection StringToFeatureCollection(string content)
    {
        return JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(content);
    }

    public static string FeatureCollectionToString(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        return JsonConvert.SerializeObject(featureCollection)
            .Replace("\"type\":8", "\"type\":\"FeatureCollection\"")
            .Replace("\"type\":7", "\"type\":\"Feature\"")
            .Replace("\"type\":5", "\"type\":\"MultiPolygon\"")
            .Replace("\"type\":4", "\"type\":\"Polygon\"")
            .Replace("\"type\":3", "\"type\":\"MultiLineString\"")
            .Replace("\"type\":0", "\"type\":\"Point\"");
    }

    public static void FeaturesToFile(string filename, List<GeoJSON.Net.Feature.Feature> features)
    {
        GeoJSON.Net.Feature.FeatureCollection featureCollection = new GeoJSON.Net.Feature.FeatureCollection();
        featureCollection.Features = features;

        StreamWriter sw = new StreamWriter(filename);
        sw.WriteLine(FeatureCollectionToString(featureCollection));
        sw.Close();
    }
}
