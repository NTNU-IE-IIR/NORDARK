
using UnityEngine;
using UnityEngine.Assertions;

public class DataVisualizationTabControl : TabControl
{
    [SerializeField] private GameObject legend;

    void Awake()
    {
        Assert.IsNotNull(legend);
    }

    public override void OnTabOpened()
    {
        legend.SetActive(true);
    }

    public override void OnTabClosed()
    {
        legend.SetActive(false);
    }
}