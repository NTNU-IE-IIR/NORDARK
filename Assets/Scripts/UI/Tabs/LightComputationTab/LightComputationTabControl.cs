using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationTabControl : TabControl
{
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private MeasureLineParametersWindow measureLineParametersWindow;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(measureLineParametersWindow);
    }

    public override void OnTabOpened()
    {
        lightComputationManager.Open();
    }

    public override void OnTabClosed()
    {
        lightComputationManager.Close();
        measureLineParametersWindow.Close();
    }
}