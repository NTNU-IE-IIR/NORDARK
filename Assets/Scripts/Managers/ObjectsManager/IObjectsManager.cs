using System.Collections.Generic;

public interface IObjectsManager
{
    void Create(GeoJSON.Net.Feature.Feature feature);
    void Clear();
    void OnLocationChanged();
    List<GeoJSON.Net.Feature.Feature> GetFeatures();
}
