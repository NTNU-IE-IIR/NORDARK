using System.Collections.Generic;

public class Dataset
{
    public GeoJSON.Net.Feature.FeatureCollection FeatureCollection;
    public List<VisualizationFeature> VisualizationFeatures;

    public Dataset(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        FeatureCollection = featureCollection;
        VisualizationFeatures = new List<VisualizationFeature>();
    }
}
