using UnityEngine;
using UnityEngine.Assertions;

public class LightsTabControl : TabControl
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightPolesSelectionManager lightPolesSelectionManager;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(lightPolesSelectionManager);
    }

    public override void OnTabOpened()
    {}

    public override void OnTabClosed()
    {
        lightPolesManager.ClearSelectedObjects();
        lightPolesSelectionManager.Stop();
    }
}
