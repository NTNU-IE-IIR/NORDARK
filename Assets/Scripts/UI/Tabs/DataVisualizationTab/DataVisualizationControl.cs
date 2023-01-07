using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

using Newtonsoft.Json;

public class DataVisualizationControl : MonoBehaviour
{
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

    public void AddDataset(string datasetName, List<string> indicators)
    {
        DatasetControl datasetControl = Instantiate(indicatorPrefab, indicatorsHolder).GetComponent<DatasetControl>();
        datasetControl.Create(datasetName, indicators, isOn => {
            dataVisualizationManager.DisplayDataset(datasetName, isOn);
        }, indicator => {
            dataVisualizationManager.SetCurrentIndicator(datasetName, indicator);
        }, () => {
            dataVisualizationManager.DeleteDataset(datasetName);
        });
        datasetControls.Add(datasetName, datasetControl);

        indicatorsHolder.sizeDelta += new Vector2(0, datasetControl.GetHeight());
    }

    public void DeleteDataset(string datasetName)
    {
        indicatorsHolder.sizeDelta -= new Vector2(0, datasetControls[datasetName].GetHeight());

        Destroy(datasetControls[datasetName].gameObject);
        datasetControls.Remove(datasetName);
    }

    private void AddDataSets()
    {
        displayDatasets.isOn = true;
        
        List<string> addedDatasets = new List<string>();
        List<string> notAddedDatasets = new List<string>();

        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select a GeoJSON file", "", "geojson", false);
        foreach (string path in paths) {
            string datasetName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (dataVisualizationManager.CreateDatasetFromNameAndPath(datasetName, path)) {
                addedDatasets.Add(datasetName);
            } else {
                notAddedDatasets.Add(datasetName);
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
