using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationTabControl : TabControl
{
    [SerializeField] private LightComputationManager lightComputationManager;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
    }

    public override void OnTabOpened()
    {
        lightComputationManager.Open();
    }

    public override void OnTabClosed()
    {
        lightComputationManager.Close();
    }
}