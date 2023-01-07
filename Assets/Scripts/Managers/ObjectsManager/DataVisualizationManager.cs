using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class DataVisualizationManager : MonoBehaviour, IObjectsManager
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private DataVisualizationControl dataVisualizationControl;
    [SerializeField] private GameObject visualizationFeaturePrefab;
    [SerializeField] private Transform datasetsParent;
    private Dictionary<string, Dataset> datasets;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(dataVisualizationControl);
        Assert.IsNotNull(visualizationFeaturePrefab);
        Assert.IsNotNull(datasetsParent);

        datasets = new Dictionary<string, Dataset>();
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
    {
        string name = "";
        if (feature.Properties.ContainsKey("name")) {
            name = feature.Properties["name"] as string;
        }
        string content = "";
        if (feature.Properties.ContainsKey("content")) {
            content = feature.Properties["content"] as string;
        }

        AddDataset(name, new Dataset(GeoJSONParser.StringToFeatureCollection(content)));
    }

    public void Clear()
    {
        ClearVisualizationFeatures();
        datasets.Clear();
    }

    public void OnLocationChanged()
    {
        ClearVisualizationFeatures();

        foreach (KeyValuePair<string, Dataset> dataset in datasets) {
            CreateVisualizationFeatures(dataset.Key, dataset.Value);
        }
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (KeyValuePair<string, Dataset> dataset in datasets) {            
            GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(0, 0, 0));

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("type", "dataset");
            properties.Add("name", dataset.Key);
            properties.Add("content", GeoJSONParser.FeatureCollectionToString(dataset.Value.FeatureCollection));
            
            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
        }

        return features;
    }

    public bool CreateDatasetFromNameAndPath(string datasetName, string path)
    {
        return AddDataset(datasetName, new Dataset(GeoJSONParser.FileToFeatureCollection(path)));
    }

    public void DeleteDataset(string datasetName)
    {
        if (datasets.ContainsKey(datasetName)) {
            ClearVisualizationFeature(datasetName);
            datasets.Remove(datasetName);
        }
    }

    public void SetCurrentIndicator(string datasetName, string indicator)
    {
        foreach(VisualizationFeature visualizationFeature in datasets[datasetName].VisualizationFeatures) {
            visualizationFeature.SetCurrentIndicator(indicator);
        }
    }

    public void DisplayDataset(string datasetName, bool display)
    {
        foreach(VisualizationFeature visualizationFeature in datasets[datasetName].VisualizationFeatures) {
            visualizationFeature.gameObject.SetActive(display);
        }
    }

    public void DisplayDatasets(bool display)
    {
        datasetsParent.gameObject.SetActive(display);
    }

    private bool AddDataset(string datasetName, Dataset dataset)
    {
        if (datasets.ContainsKey(datasetName)) {
            return false;
        } else {
            datasets[datasetName] = dataset;
            CreateVisualizationFeatures(datasetName, dataset);
            return true;
        }
    }

    private void CreateVisualizationFeatures(string datasetName, Dataset dataset)
    {
        HashSet<string> propertyNames = new HashSet<string>();

        foreach (GeoJSON.Net.Feature.Feature feature in dataset.FeatureCollection.Features) {
            if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.LineString")) {
                VisualizationFeature visualizationFeature = Instantiate(visualizationFeaturePrefab, datasetsParent).GetComponent<VisualizationFeature>();
                visualizationFeature.Create(feature, mapManager);
                
                if (visualizationFeature.IsCreated()) {
                    dataset.VisualizationFeatures.Add(visualizationFeature);
                }
            }

            foreach (KeyValuePair<string, object> property in feature.Properties) {
                try {
                    double indicator = (double) property.Value;
                    propertyNames.Add(property.Key);
                } catch (System.Exception) {}
            }
        }

        dataVisualizationControl.AddDataset(datasetName, propertyNames.ToList());

        if (propertyNames.Count > 0) {
            SetCurrentIndicator(datasetName, propertyNames.ElementAt(0));
        }
    }

    private void ClearVisualizationFeatures()
    {
        foreach (KeyValuePair<string, Dataset> dataset in datasets) {
            ClearVisualizationFeature(dataset.Key);
        }
    }

    private void ClearVisualizationFeature(string datasetName)
    {
        foreach(VisualizationFeature visualizationFeature in datasets[datasetName].VisualizationFeatures) {
            Destroy(visualizationFeature.gameObject);
        }
        dataVisualizationControl.DeleteDataset(datasetName);

        datasets[datasetName].VisualizationFeatures.Clear();
    }
}
