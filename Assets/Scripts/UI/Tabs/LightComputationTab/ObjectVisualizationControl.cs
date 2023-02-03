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
        Assert.IsNotNull(exportResults);
        Assert.IsNotNull(displayVisualizationMethod);
        Assert.IsNotNull(resolution); 
    }

    void Start()
    {
        createMeasureObject.onClick.AddListener(computationObject.Draw);

        if (importResults != null) {
            importResults.onClick.AddListener(computationObject.ImportResults);
        }
        
        exportResults.onClick.AddListener(computationObject.ExportResults);
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
}