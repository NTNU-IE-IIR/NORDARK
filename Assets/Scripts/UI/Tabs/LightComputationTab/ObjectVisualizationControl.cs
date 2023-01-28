using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class ObjectVisualizationControl : MonoBehaviour
{
    [SerializeField] private GameObject computationObjectGameObject;
    [SerializeField] private Button createMeasureObject;
    [SerializeField] private Button importResults;
    [SerializeField] private Button exportResults;
    [SerializeField] private Toggle displayVisualizationMethod;
    [SerializeField] private TMP_InputField resolution;
    private IComputationObject computationObject;

    void Awake()
    {
        Assert.IsNotNull(computationObjectGameObject);
        Assert.IsNotNull(createMeasureObject);
        Assert.IsNotNull(importResults);
        Assert.IsNotNull(exportResults);
        Assert.IsNotNull(displayVisualizationMethod);
        Assert.IsNotNull(resolution);

        // The gameobject is used because IComputationObject is an interface and cannot be serialized
        computationObject = computationObjectGameObject.GetComponent<IComputationObject>();
        Assert.IsNotNull(computationObject);
    }

    void Start()
    {
        createMeasureObject.onClick.AddListener(computationObject.Draw);
        importResults.onClick.AddListener(computationObject.ImportResults);
        exportResults.onClick.AddListener(computationObject.ExportResults);
        displayVisualizationMethod.onValueChanged.AddListener(computationObject.ShowVisualizationMethod);
    }

    public IComputationObject GetComputationObject()
    {
        return computationObject;
    }

    public int GetResolution()
    {
        return System.Int32.Parse(resolution.text);
    }
}