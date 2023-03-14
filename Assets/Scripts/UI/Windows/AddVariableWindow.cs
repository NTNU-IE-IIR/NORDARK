using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class AddVariableWindow : MonoBehaviour
{
    [SerializeField] private Button close;
    [SerializeField] private TMP_Text selectedFile;
    [SerializeField] private Button browse;
    [SerializeField] private TMP_Dropdown variable;
    [SerializeField] private Button addVariable;
    [SerializeField] private TMP_Text error;
    private System.Action<string, string> onVariableAdded;
    private string path;

    void Awake()
    {
        Assert.IsNotNull(close);
        Assert.IsNotNull(selectedFile);
        Assert.IsNotNull(browse);
        Assert.IsNotNull(variable);
        Assert.IsNotNull(addVariable);
        Assert.IsNotNull(error);
    }

    void Start()
    {
        close.onClick.AddListener(Close);
        browse.onClick.AddListener(Browse);
        addVariable.onClick.AddListener(AddVariable);
    }

    public void Open(System.Action<string, string> onVariableAdded)
    {
        this.onVariableAdded = onVariableAdded;
        gameObject.SetActive(true);
    }

    private void Close()
    {
        selectedFile.text = "No file selected";
        variable.ClearOptions();
        error.text = "";
        gameObject.SetActive(false);
    }

    private void Browse()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Select a GeoJSON file", "", "geojson", false);

        if (paths.Length > 0) {
            path = paths[0];
            selectedFile.text = path;

            GeoJSON.Net.Feature.FeatureCollection features = GeoJSONParser.FileToFeatureCollection(path);
            HashSet<string> variables = new HashSet<string>();
            foreach (GeoJSON.Net.Feature.Feature feature in features.Features) {
                foreach (KeyValuePair<string, object> property in feature.Properties) {
                    try {
                        double indicator = (double) property.Value;
                        variables.Add(property.Key);
                    } catch (System.Exception) {}
                }
            }
            variable.ClearOptions();
            variable.AddOptions(variables.ToList());
        }
    }
    
    private void AddVariable()
    {
        if (variable.value < 0 || variable.value >= variable.options.Count || variable.options[variable.value].text == "") {
            error.text = "Select a variable";
        } else {
            onVariableAdded(variable.options[variable.value].text, path);
            Close();
        }
    }
}
