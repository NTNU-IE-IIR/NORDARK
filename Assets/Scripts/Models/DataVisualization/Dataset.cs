using System.Collections.Generic;

public class Dataset
{
    public GeoJSON.Net.Feature.FeatureCollection FeatureCollection;
    public List<VisualizationFeature> VisualizationFeatures;
    public Dictionary<string, float> Weights;

    public Dataset(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        FeatureCollection = featureCollection;
        VisualizationFeatures = new List<VisualizationFeature>();

        Weights = new Dictionary<string, float>();
        foreach (GeoJSON.Net.Feature.Feature feature in FeatureCollection.Features) {
            foreach (KeyValuePair<string, object> property in feature.Properties) {
                try {
                    double indicator = (double) property.Value;
                    Weights[property.Key] = 1;
                } catch (System.Exception) {}
            }
        }
    }
}
