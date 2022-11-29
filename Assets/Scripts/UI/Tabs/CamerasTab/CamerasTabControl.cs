using UnityEngine;
using UnityEngine.Assertions;

public class CamerasTabControl : TabControl
{
    [SerializeField] private CamerasManager camerasManager;

    void Awake()
    {
        Assert.IsNotNull(camerasManager);
    }

    public override void OnTabOpened()
    {
        camerasManager.DisplayCameraPreview(true);
    }

    public override void OnTabClosed()
    {
        camerasManager.DisplayCameraPreview(false);
    }
}