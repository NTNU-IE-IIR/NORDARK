using System.Collections.Generic;

public class GroundTextureCollection
{
    public string Id;   // unique id to identify each ground texture collection
    public List<GroundTexture> GroundTextures;
    public GeoJSON.Net.Feature.FeatureCollection FeatureCollection;
    public Location Location;

    public GroundTextureCollection(GeoJSON.Net.Feature.FeatureCollection featureCollection, string id, Location location)
    {
        if (id == "") {
            Id = System.Guid.NewGuid().ToString();
        } else {
            Id = id;
        }
        
        GroundTextures = new List<GroundTexture>();
        FeatureCollection = featureCollection;
        Location = location;
    }
}
