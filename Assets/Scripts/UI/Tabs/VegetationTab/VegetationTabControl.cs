using UnityEngine;
using UnityEngine.Assertions;

public class VegetationTabControl : TabControl
{
    [SerializeField] private VegetationObjectsManager vegetationObjectsManager;

    void Awake()
    {
        Assert.IsNotNull(vegetationObjectsManager);
    }
    public override void OnTabOpened()
    {}

    public override void OnTabClosed()
    {
        vegetationObjectsManager.ClearSelectedObjects();
    }
}