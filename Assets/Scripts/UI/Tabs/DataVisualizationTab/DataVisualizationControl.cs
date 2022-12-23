using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

using Newtonsoft.Json;

public class DataVisualizationControl : MonoBehaviour
{
    public bool start = false;
    public string indicators = "indicators";


    [SerializeField] private DataVisualizationManager dataVisualizationManager;
    [SerializeField] private RectTransform indicatorsHolder;
    [SerializeField] private DialogControl dialogControl;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Toggle displayDatasets;
    [SerializeField] private Button addDataset;
    private Dictionary<string, DatasetControl> datasetControls;

    void Awake()
    {
        Assert.IsNotNull(dataVisualizationManager);
        Assert.IsNotNull(indicatorsHolder);
        Assert.IsNotNull(dialogControl);
        Assert.IsNotNull(indicatorPrefab);
        Assert.IsNotNull(displayDatasets);
        Assert.IsNotNull(addDataset);

        datasetControls = new Dictionary<string, DatasetControl>();
    }

    void Start()
    {
        addDataset.onClick.AddListener(AddDataSets);
        displayDatasets.onValueChanged.AddListener(dataVisualizationManager.DisplayDatasets);
    }
    
    void Update()
    {
        if (start) {
            start = false;
            AddDataSets();
        }
    }

    public void AddDataset(string dataset, List<string> indicators)
    {
        DatasetControl datasetControl = Instantiate(indicatorPrefab, indicatorsHolder).GetComponent<DatasetControl>();
        datasetControl.Create(dataset, indicators, isOn => {
            dataVisualizationManager.DisplayDataset(dataset, isOn);
        }, indicator => {
            dataVisualizationManager.SetCurrentIndicator(dataset, indicator);
        }, () => {
            dataVisualizationManager.DeleteDataset(dataset);
        });
        datasetControls.Add(dataset, datasetControl);

        indicatorsHolder.sizeDelta = indicatorsHolder.sizeDelta + new Vector2(0, datasetControl.GetHeight());
    }

    public void DeleteDataset(string dataset)
    {
        Destroy(datasetControls[dataset].gameObject);
        datasetControls.Remove(dataset);
    }

    private void AddDataSets()
    {
        displayDatasets.isOn = true;

        List<string> addedDatasets = new List<string>();
        List<string> notAddedDatasets = new List<string>();

        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select a GeoJSON file", "", "geojson", false);
        foreach (string path in paths) {
            string dataset = System.IO.Path.GetFileNameWithoutExtension(path);
            GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONReader.ReadFile(path);

            if (dataVisualizationManager.AddDataset(dataset, featureCollection)) {
                addedDatasets.Add(dataset);
            } else {
                notAddedDatasets.Add(dataset);
            }
        }
        
        string message = "";
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
            message += "Datasets with the same name already exist or no indicator value has been found.";
        }
        if (message != "") {
            dialogControl.CreateInfoDialog(message);
        }
    }
}
