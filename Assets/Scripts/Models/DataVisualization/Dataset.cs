using System.Collections.Generic;

public class Dataset
{
    public GeoJSON.Net.Feature.FeatureCollection FeatureCollection;
    public List<VisualizationFeature> VisualizationFeatures;
    public Dictionary<string, float> Weights;
    public Location Location;

    public Dataset(GeoJSON.Net.Feature.FeatureCollection featureCollection, Location location)
    {
        FeatureCollection = featureCollection;
        Location = location;
        VisualizationFeatures = new List<VisualizationFeature>();

        // Only add a variable if all values are between 0 and 1
        HashSet<string> potentiallyValidVariables = new HashSet<string>();
        HashSet<string> nonValidVariables = new HashSet<string>();
        foreach (GeoJSON.Net.Feature.Feature feature in FeatureCollection.Features) {
            foreach (string property in feature.Properties.Keys) {
                // An exception is thrown if the property is not a double
                try {
                    double propertyValue = (double) feature.Properties[property];
                    if (propertyValue >= 0 && propertyValue <= 1) {
                        potentiallyValidVariables.Add(property);
                    } else {
                        nonValidVariables.Add(property);
                    }
                } catch (System.Exception) {}
            }
        }
        
        Weights = new Dictionary<string, float>();
        foreach (string variable in potentiallyValidVariables) {
            if (!nonValidVariables.Contains(variable)) {
                Weights.Add(variable, 1);
            }
        }
    }
}
