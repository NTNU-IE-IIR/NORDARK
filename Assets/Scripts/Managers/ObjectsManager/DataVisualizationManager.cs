using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DataVisualizationManager : MonoBehaviour, IObjectsManager
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private DataVisualizationControl dataVisualizationControl;
    [SerializeField] private GameObject visualizationFeaturePrefab;
    [SerializeField] private Transform datasetsParent;
    private Dictionary<string, List<VisualizationFeature>> visualizationFeatures;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(dataVisualizationControl);
        Assert.IsNotNull(visualizationFeaturePrefab);
        Assert.IsNotNull(datasetsParent);

        visualizationFeatures = new Dictionary<string, List<VisualizationFeature>>();
    }

    public void Create(Feature feature)
    {

    }

    public void Clear()
    {
        
    }

    public void OnLocationChanged()
    {
        
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        return features;
    }

    public bool AddDataset(string dataset, GeoJSON.Net.Feature.FeatureCollection featureCollection)
    {
        if (visualizationFeatures.ContainsKey(dataset)) {
            return false;
        } else {
            List<string> propertyNames = null;
            visualizationFeatures[dataset] = new List<VisualizationFeature>();

            foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.LineString")) {
                    VisualizationFeature visualizationFeature = Instantiate(visualizationFeaturePrefab, datasetsParent).GetComponent<VisualizationFeature>();
                    if (visualizationFeature.Create(feature, mapManager)) {
                        visualizationFeatures[dataset].Add(visualizationFeature);
                    }
                }

                if (propertyNames == null) {
                    propertyNames = new List<string>();

                    foreach (KeyValuePair<string, object> property in feature.Properties) {
                        try {
                            double indicator = (double) property.Value;
                            propertyNames.Add(property.Key);
                        } catch (System.Exception) {}
                    }
                }
            }

            if (propertyNames != null && propertyNames.Count > 0) {
                dataVisualizationControl.AddDataset(dataset, propertyNames);
                SetCurrentIndicator(dataset, propertyNames[0]);
                return true;
            } else {
                visualizationFeatures.Remove(dataset);
                return false;
            }
        }
    }

    public void DeleteDataset(string dataset)
    {
        foreach(VisualizationFeature visualizationFeature in visualizationFeatures[dataset]) {
            Destroy(visualizationFeature.gameObject);
        }
        visualizationFeatures.Remove(dataset);
        dataVisualizationControl.DeleteDataset(dataset);
    }

    public void SetCurrentIndicator(string dataset, string indicator)
    {
        foreach(VisualizationFeature visualizationFeature in visualizationFeatures[dataset]) {
            visualizationFeature.SetCurrentIndicator(indicator);
        }
    }

    public void DisplayDataset(string dataset, bool display)
    {
        foreach(VisualizationFeature visualizationFeature in visualizationFeatures[dataset]) {
            visualizationFeature.gameObject.SetActive(display);
        }
    }

    public void DisplayDatasets(bool display)
    {
        datasetsParent.gameObject.SetActive(display);
    }
}
