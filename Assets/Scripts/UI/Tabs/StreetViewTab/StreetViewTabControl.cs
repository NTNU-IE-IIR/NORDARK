using UnityEngine;
using UnityEngine.Assertions;

public class StreetViewTabControl : TabControl
{
    [SerializeField] private StreetViewManager streetViewManager;

    void Awake()
    {
        Assert.IsNotNull(streetViewManager);
    }

    public override void OnTabOpened()
    {
        streetViewManager.DisplayCameraPreview(true);
    }

    public override void OnTabClosed()
    {
        streetViewManager.DisplayCameraPreview(false);
    }
}