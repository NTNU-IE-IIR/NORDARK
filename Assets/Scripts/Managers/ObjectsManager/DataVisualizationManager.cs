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

    public bool AddVariable(string datasetName, string variable, string path)
    {
        if (datasets.ContainsKey(datasetName)) {
            Dataset dataset = datasets[datasetName];

            if (dataset.Weights.ContainsKey(variable)) {
                return false;
            }

            GeoJSON.Net.Feature.FeatureCollection features = GeoJSONParser.FileToFeatureCollection(path);

            Dictionary<GeoJSON.Net.Feature.Feature, double> newValues = new Dictionary<GeoJSON.Net.Feature.Feature, double>();
            double maxNewValue = -Mathf.Infinity;
            foreach (GeoJSON.Net.Feature.Feature feature in features.Features) {
                GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                if (point != null) {
                    GeoJSON.Net.Feature.Feature nearestFeature = FindNearestFeatureFromPosition(
                        new Vector3d(point.Coordinates),
                        dataset.FeatureCollection
                    );
                    if (nearestFeature != null) {
                        double variableValue = (double) feature.Properties[variable];
                        maxNewValue = System.Math.Max(maxNewValue, variableValue);

                        if (newValues.ContainsKey(nearestFeature)) {
                            newValues[nearestFeature] += variableValue;
                            newValues[nearestFeature] /= 2;
                        } else {
                            newValues[nearestFeature] = variableValue;
                        }
                    }
                }
            }

            foreach (GeoJSON.Net.Feature.Feature feature in newValues.Keys.ToList()) {
                newValues[feature] /= maxNewValue;
            }

            foreach (GeoJSON.Net.Feature.Feature feature in dataset.FeatureCollection.Features) {
                if (newValues.ContainsKey(feature)) {
                    feature.Properties[variable] = newValues[feature];
                } else {
                    feature.Properties[variable] = .0;
                }
            }

            DeleteDataset(datasetName);
            AddDataset(datasetName, dataset);

            return true;
        } else {
            return false;
        }
    }

    public void DeleteDataset(string datasetName)
    {
        if (datasets.ContainsKey(datasetName)) {
            ClearVisualizationFeature(datasetName);
            datasets.Remove(datasetName);
        }
    }

    public void SetIndicatorWeight(string datasetName, string indicator, float weight)
    {
        datasets[datasetName].Weights[indicator] = weight;

        foreach(VisualizationFeature visualizationFeature in datasets[datasetName].VisualizationFeatures) {
            visualizationFeature.SetWeights(datasets[datasetName].Weights);
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
            bool atLeastOneValidFeature = false;
            foreach (GeoJSON.Net.Feature.Feature feature in dataset.FeatureCollection.Features) {
                GeoJSON.Net.Geometry.LineString lineString = GetLineStringFromGeometry(feature.Geometry);

                if (lineString != null && Utils.IsEPSG4326(lineString.Coordinates[0])) {
                    atLeastOneValidFeature = true;
                }
            }
            if (atLeastOneValidFeature) {
                datasets[datasetName] = dataset;
                CreateVisualizationFeatures(datasetName, dataset);
                return true;
            } else {
                return false;
            }
        }
    }

    private void ClearVisualizationFeatures()
    {
        foreach (KeyValuePair<string, Dataset> dataset in datasets) {
            ClearVisualizationFeature(dataset.Key);
        }
    }

    private void CreateVisualizationFeatures(string datasetName, Dataset dataset)
    {
        foreach (GeoJSON.Net.Feature.Feature feature in dataset.FeatureCollection.Features) {
            if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.LineString") || string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiLineString")) {
                VisualizationFeature visualizationFeature = Instantiate(visualizationFeaturePrefab, datasetsParent).GetComponent<VisualizationFeature>();
                visualizationFeature.Create(datasetName, dataset.Weights, feature, mapManager);
                
                if (visualizationFeature.IsCreated()) {
                    dataset.VisualizationFeatures.Add(visualizationFeature);
                }
            }
        }

        dataVisualizationControl.AddDataset(datasetName, dataset.Weights.Keys.ToList());
    }

    private GeoJSON.Net.Feature.Feature FindNearestFeatureFromPosition(Vector3d position, GeoJSON.Net.Feature.FeatureCollection features)
    {
        GeoJSON.Net.Feature.Feature nearestFeature = null;
        double minDistance = Mathf.Infinity;

        foreach (GeoJSON.Net.Feature.Feature feature in features.Features) {
            GeoJSON.Net.Geometry.LineString lineString = GetLineStringFromGeometry(feature.Geometry);

            if (lineString != null) {
                foreach (GeoJSON.Net.Geometry.IPosition point in lineString.Coordinates) {
                    double distance = Vector3d.Distance(position, new Vector3d(point));
                    if (distance < minDistance) {
                        minDistance = distance;
                        nearestFeature = feature;
                    }
                }
            }
        }

        return nearestFeature;
    }

    private void ClearVisualizationFeature(string datasetName)
    {
        foreach(VisualizationFeature visualizationFeature in datasets[datasetName].VisualizationFeatures) {
            Destroy(visualizationFeature.gameObject);
        }
        dataVisualizationControl.DeleteDataset(datasetName);

        datasets[datasetName].VisualizationFeatures.Clear();
    }

    private GeoJSON.Net.Geometry.LineString GetLineStringFromGeometry(GeoJSON.Net.Geometry.IGeometryObject geometry)
    {
        GeoJSON.Net.Geometry.LineString lineString = null;
        if (string.Equals(geometry.GetType().FullName, "GeoJSON.Net.Geometry.LineString")) {
            lineString = geometry as GeoJSON.Net.Geometry.LineString;
        } else if (string.Equals(geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiLineString")) {
            lineString = (geometry as GeoJSON.Net.Geometry.MultiLineString).Coordinates[0];
        }

        return lineString;
    }
}
