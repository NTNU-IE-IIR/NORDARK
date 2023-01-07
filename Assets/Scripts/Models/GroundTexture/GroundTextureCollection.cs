using System.Collections.Generic;

public class GroundTextureCollection
{
    public string Id;
    public List<GroundTexture> GroundTextures;
    public GeoJSON.Net.Feature.FeatureCollection FeatureCollection;

    public GroundTextureCollection(GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        Id = System.Guid.NewGuid().ToString();
        GroundTextures = new List<GroundTexture>();
        FeatureCollection = featureCollection;
    }
}
