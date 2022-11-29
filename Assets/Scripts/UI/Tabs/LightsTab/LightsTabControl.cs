using UnityEngine;
using UnityEngine.Assertions;

public class LightsTabControl : TabControl
{
    [SerializeField] private LightsManager lightsManager;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
    }

    public override void OnTabOpened()
    {}

    public override void OnTabClosed()
    {
        lightsManager.ClearSelectedLight();
    }
}
