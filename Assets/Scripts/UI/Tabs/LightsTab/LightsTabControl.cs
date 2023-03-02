using UnityEngine;
using UnityEngine.Assertions;

public class LightsTabControl : TabControl
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightPolesSelectionManager lightPolesSelectionManager;
    private bool isActive;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(lightPolesSelectionManager);

        isActive = false;
    }

    public override void OnTabOpened()
    {
        isActive = true;
    }

    public override void OnTabClosed()
    {
        isActive = false;
        lightPolesManager.ClearSelectedLightPoles();
        lightPolesSelectionManager.Stop();
    }

    public bool IsActive()
    {
        return isActive;
    }
}
