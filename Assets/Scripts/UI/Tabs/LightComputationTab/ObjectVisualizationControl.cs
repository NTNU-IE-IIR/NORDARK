using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class ObjectVisualizationControl : MonoBehaviour
{
    [SerializeField] private GameObject computationObjectGameObject;
    [SerializeField] private GameObject measureParametersWindow;
    [SerializeField] private Button createMeasureObjectManually;
    [SerializeField] private Button createMeasureObjectWithParameters;
    [SerializeField] private Button importResults;
    [SerializeField] private Button exportResultsGeoJSON;
    [SerializeField] private Button exportResultsCSV;
    [SerializeField] private TMP_InputField minValue;
    [SerializeField] private Toggle minValueAuto;
    [SerializeField] private TMP_InputField maxValue;
    [SerializeField] private Toggle maxValueAuto;
    [SerializeField] private TMP_InputField resolution;
    [SerializeField] private Toggle displayValues;
    [SerializeField] private Toggle displayVisualizationMethod;
    private IComputationObject computationObject;

    void Awake()
    {
        Assert.IsNotNull(computationObjectGameObject);
        Assert.IsNotNull(createMeasureObjectManually);
        Assert.IsNotNull(exportResultsGeoJSON);
        Assert.IsNotNull(exportResultsCSV);
        Assert.IsNotNull(minValue);
        Assert.IsNotNull(minValueAuto);
        Assert.IsNotNull(maxValue);
        Assert.IsNotNull(maxValueAuto);
        Assert.IsNotNull(resolution);
        Assert.IsNotNull(displayVisualizationMethod);
    }

    void Start()
    {
        createMeasureObjectManually.onClick.AddListener(computationObject.Draw);

        if (createMeasureObjectWithParameters != null) {
            createMeasureObjectWithParameters.onClick.AddListener(() => {
                measureParametersWindow.SetActive(true);
            });
        }

        if (importResults != null) {
            importResults.onClick.AddListener(computationObject.ImportResults);
        }
        
        exportResultsGeoJSON.onClick.AddListener(computationObject.ExportResultsGeoJSON);
        exportResultsCSV.onClick.AddListener(computationObject.ExportResultsCSV);

        minValueAuto.onValueChanged.AddListener(SetMinAuto);
        maxValueAuto.onValueChanged.AddListener(SetMaxAuto);

        if (displayValues != null) {
            displayValues.onValueChanged.AddListener(computationObject.DisplayValues);
        }

        displayVisualizationMethod.onValueChanged.AddListener(computationObject.ShowVisualizationMethod);
    }

    public IComputationObject GetComputationObject()
    {
        // The gameobject is used because IComputationObject is an interface and cannot be serialized
        if (computationObject == null) {
            computationObject = computationObjectGameObject.GetComponent<IComputationObject>();
        }
        return computationObject;
    }

    public int GetResolution()
    {
        return System.Int32.Parse(resolution.text);
    }

    public void SetResolution(int newResolution)
    {
        resolution.text = newResolution.ToString();
    }

    public bool isMinAuto()
    {
        return minValueAuto.isOn;
    }

    public float GetMinValue()
    {
        return float.Parse(minValue.text);
    }

    public bool isMaxAuto()
    {
        return maxValueAuto.isOn;
    }

    public float GetMaxValue()
    {
        return float.Parse(maxValue.text);
    }

    private void SetMinAuto(bool setAuto)
    {
        minValue.interactable = !setAuto;
    }

    private void SetMaxAuto(bool setAuto)
    {
        maxValue.interactable = !setAuto;
    }
}