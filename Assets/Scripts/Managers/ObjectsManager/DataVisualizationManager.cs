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

            Dictionary<string, object> properties = new Dictionary<string, object> {
                {"type", "dataset"},
                {"name", dataset.Key},
                {"content", GeoJSONParser.FeatureCollectionToString(dataset.Value.FeatureCollection)}
            };
            
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

            dataset.Weights[variable] = 1;

            // Determine which features of the current dataset are closer to the new variable dataset and assign their values
            GeoJSON.Net.Feature.FeatureCollection features = GeoJSONParser.FileToFeatureCollection(path);
            Dictionary<GeoJSON.Net.Feature.Feature, double> featuresWithNewVariableValues = new Dictionary<GeoJSON.Net.Feature.Feature, double>();
            Dictionary<GeoJSON.Net.Feature.Feature, int> featuresWithNewVariableCount = new Dictionary<GeoJSON.Net.Feature.Feature, int>();
            double maxNewValue = -Mathf.Infinity;
            foreach (GeoJSON.Net.Feature.Feature feature in features.Features) {
                // Only points are supported
                GeoJSON.Net.Geometry.Point point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                if (point != null) {
                    GeoJSON.Net.Feature.Feature nearestFeature = FindNearestFeatureFromPosition(
                        new Coordinate(point.Coordinates),
                        dataset.FeatureCollection
                    );
                    if (nearestFeature != null) {
                        double variableValue = (double) feature.Properties[variable];
                        maxNewValue = System.Math.Max(maxNewValue, variableValue);

                        if (featuresWithNewVariableValues.ContainsKey(nearestFeature)) {
                            featuresWithNewVariableValues[nearestFeature] += variableValue;
                            featuresWithNewVariableCount[nearestFeature]++;
                        } else {
                            featuresWithNewVariableValues[nearestFeature] = variableValue;
                            featuresWithNewVariableCount[nearestFeature] = 1;
                        }
                    }
                }
            }
            foreach (GeoJSON.Net.Feature.Feature feature in featuresWithNewVariableValues.Keys.ToList()) {
                featuresWithNewVariableValues[feature] /= maxNewValue * featuresWithNewVariableCount[feature];
            }

            foreach (GeoJSON.Net.Feature.Feature feature in dataset.FeatureCollection.Features) {
                if (featuresWithNewVariableValues.ContainsKey(feature)) {
                    feature.Properties[variable] = featuresWithNewVariableValues[feature];
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

    public void SetVariableWeight(string datasetName, string variable, float weight)
    {
        datasets[datasetName].Weights[variable] = weight;

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
        if (datasets.ContainsKey(datasetName) || dataset.Weights.Count == 0) {
            return false;
        } else {
            bool atLeastOneValidFeature = false;
            int i = 0;
            while (i < dataset.FeatureCollection.Features.Count && !atLeastOneValidFeature) {
                GeoJSON.Net.Feature.Feature feature = dataset.FeatureCollection.Features[i];
                ApplyActionToEachPointOfGeometry(feature.Geometry, point => {
                    atLeastOneValidFeature = atLeastOneValidFeature || Utils.IsEPSG4326(point);
                });

                i++;
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
        // Reset weights
        foreach (string variable in dataset.Weights.Keys.ToList()) {
            dataset.Weights[variable] = 1;
        }

        foreach (GeoJSON.Net.Feature.Feature feature in dataset.FeatureCollection.Features) {
            VisualizationFeature visualizationFeature = Instantiate(visualizationFeaturePrefab, datasetsParent).GetComponent<VisualizationFeature>();
            visualizationFeature.Create(datasetName, dataset.Weights, feature, mapManager);
            
            if (visualizationFeature.IsCreated()) {
                dataset.VisualizationFeatures.Add(visualizationFeature);
            }
        }

        dataVisualizationControl.AddDataset(datasetName, dataset.Weights.Keys.ToList());
    }

    private GeoJSON.Net.Feature.Feature FindNearestFeatureFromPosition(Coordinate position, GeoJSON.Net.Feature.FeatureCollection features)
    {
        GeoJSON.Net.Feature.Feature nearestFeature = null;
        double minDistance = Mathf.Infinity;

        foreach (GeoJSON.Net.Feature.Feature feature in features.Features) {
            ApplyActionToEachPointOfGeometry(feature.Geometry, point => {
                double distance = Coordinate.Distance(position, new Coordinate(point));
                if (distance < minDistance) {
                    minDistance = distance;
                    nearestFeature = feature;
                }
            });
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

    private void ApplyActionToEachPointOfGeometry(GeoJSON.Net.Geometry.IGeometryObject geometry, System.Action<GeoJSON.Net.Geometry.IPosition> action)
    {
        switch (geometry) {
            case GeoJSON.Net.Geometry.Point point:
                action(point.Coordinates);
                break;
            case GeoJSON.Net.Geometry.MultiPoint multiPoint:
                if (multiPoint.Coordinates.Count > 0) {
                    action(multiPoint.Coordinates[0].Coordinates);
                }
                break;
            case GeoJSON.Net.Geometry.LineString lineString:
                foreach (GeoJSON.Net.Geometry.IPosition point in lineString.Coordinates) {
                    action(point);
                }
                break;
            case GeoJSON.Net.Geometry.MultiLineString multiLineString:
                if (multiLineString.Coordinates.Count > 0) {
                    foreach (GeoJSON.Net.Geometry.IPosition point in multiLineString.Coordinates[0].Coordinates) {
                        action(point);
                    }
                }
                break;
            case GeoJSON.Net.Geometry.Polygon polygon:
                foreach (GeoJSON.Net.Geometry.LineString lineString in polygon.Coordinates) {
                    foreach (GeoJSON.Net.Geometry.IPosition point in lineString.Coordinates) {
                        action(point);
                    }
                }
                break;
            case GeoJSON.Net.Geometry.MultiPolygon multiPolygon:
                if (multiPolygon.Coordinates.Count > 0) {
                    foreach (GeoJSON.Net.Geometry.LineString lineString in multiPolygon.Coordinates[0].Coordinates) {
                        foreach (GeoJSON.Net.Geometry.IPosition point in lineString.Coordinates) {
                            action(point);
                        }
                    }
                }
                break;
            default:
                break;
        }
    }
}
