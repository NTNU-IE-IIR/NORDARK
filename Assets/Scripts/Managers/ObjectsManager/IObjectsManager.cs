using System.Collections.Generic;

public interface IObjectsManager
{
    void Create(Feature feature);
    void Clear();
    void OnLocationChanged();
    List<Feature> GetFeatures();
}
