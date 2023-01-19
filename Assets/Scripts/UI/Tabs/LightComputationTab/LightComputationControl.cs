using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class LightComputationControl : MonoBehaviour
{
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private Toggle lightResults;
    [SerializeField] private Button createMeasureLine;
    [SerializeField] private Button importResults;
    [SerializeField] private Button exportResults;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(lightResults);
        Assert.IsNotNull(createMeasureLine);
        Assert.IsNotNull(importResults);
        Assert.IsNotNull(exportResults);
    }

    void Start()
    {
        lightResults.onValueChanged.AddListener(lightComputationManager.DisplayLightResults);

        createMeasureLine.onClick.AddListener(lightComputationManager.DrawLine);
        importResults.onClick.AddListener(lightComputationManager.ImportResults);
        exportResults.onClick.AddListener(lightComputationManager.ExportResults);
    }
}
