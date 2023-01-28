using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class DataVisualizationControl : MonoBehaviour
{
    [SerializeField] private DataVisualizationManager dataVisualizationManager;
    [SerializeField] private RectTransform indicatorsHolder;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private RectTransform weightsHolder;
    [SerializeField] private GameObject weightPrefab;
    [SerializeField] private Toggle displayDatasets;
    [SerializeField] private Button addDataset;
    private Dictionary<string, DatasetUI> datasetUIs = new Dictionary<string, DatasetUI>();

    void Awake()
    {
        Assert.IsNotNull(dataVisualizationManager);
        Assert.IsNotNull(indicatorsHolder);
        Assert.IsNotNull(indicatorPrefab);
        Assert.IsNotNull(weightsHolder);
        Assert.IsNotNull(weightPrefab);
        Assert.IsNotNull(displayDatasets);
        Assert.IsNotNull(addDataset);
    }

    void Start()
    {
        addDataset.onClick.AddListener(AddDataSets);
        displayDatasets.onValueChanged.AddListener(dataVisualizationManager.DisplayDatasets);
    }

    public void AddDataset(string datasetName, List<string> indicators)
    {
        DatasetUI datasetUI = new DatasetUI();

        DatasetControl datasetControl = Instantiate(indicatorPrefab, indicatorsHolder).GetComponent<DatasetControl>();
        datasetControl.Create(datasetName, isOn => {
            dataVisualizationManager.DisplayDataset(datasetName, isOn);
            foreach (WeightControl weightControl in datasetUIs[datasetName].WeightControls) {
                weightControl.gameObject.SetActive(isOn);
            }
        }, () => {
            dataVisualizationManager.DeleteDataset(datasetName);
        });
        datasetUI.DatasetControl = datasetControl;
        indicatorsHolder.sizeDelta += new Vector2(0, datasetControl.GetHeight());

        foreach (string indicator in indicators) {
            WeightControl weightControl = Instantiate(weightPrefab, weightsHolder).GetComponent<WeightControl>();
            weightControl.Create(indicator, weight => {
                dataVisualizationManager.SetIndicatorWeight(datasetName, indicator, weight);
            });
            datasetUI.WeightControls.Add(weightControl);
            weightsHolder.sizeDelta += new Vector2(0, weightControl.GetHeight());
        }
        
        datasetUIs.Add(datasetName, datasetUI);
    }

    public void DeleteDataset(string datasetName)
    {
        indicatorsHolder.sizeDelta -= new Vector2(0, datasetUIs[datasetName].DatasetControl.GetHeight());
        foreach (WeightControl weightControl in datasetUIs[datasetName].WeightControls) {
            weightsHolder.sizeDelta -= new Vector2(0, weightControl.GetHeight());
        }

        Destroy(datasetUIs[datasetName].DatasetControl.gameObject);
        foreach (WeightControl weightControl in datasetUIs[datasetName].WeightControls) {
            Destroy(weightControl.gameObject);
        }

        datasetUIs.Remove(datasetName);
    }

    private void AddDataSets()
    {
        displayDatasets.isOn = true;
        
        List<string> addedDatasets = new List<string>();
        List<string> notAddedDatasets = new List<string>();

        string message = "";
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select a GeoJSON file", "", "geojson", false);
        foreach (string path in paths) {
            string datasetName = System.IO.Path.GetFileNameWithoutExtension(path);

            try {
                if (dataVisualizationManager.CreateDatasetFromNameAndPath(datasetName, path)) {
                    addedDatasets.Add(datasetName);
                } else {
                    notAddedDatasets.Add(datasetName);
                }
            } catch (System.Exception e) {
                notAddedDatasets.Add(datasetName);
                message += e.Message + "\n";
            }
        }

        if (addedDatasets.Count > 0) {
            message += "The following datasets were added:\n";
            foreach (string file in addedDatasets) {
                message += file + "\n";
            }
        }
        if (notAddedDatasets.Count > 0) {
            message += "The following datasets were not added:\n";
            foreach (string file in notAddedDatasets) {
                message += file + "\n";
            }
            message += "Datasets with the same name already exist, no indicator value has been found, or the EPSG:4326 coordinate system was not used (longitude from -180° to 180° / latitude from -90° to 90°).";
        }
        
        if (message != "") {
            DialogControl.CreateDialog(message);
        }
    }
}