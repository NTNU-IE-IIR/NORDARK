using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private ToolBarTabsControl toolBarTabsControl;
    [SerializeField]
    private VegetationControl vegetationControl;
    [SerializeField]
    private SunControl sunControl;
    [SerializeField]
    private GameObject loadingPanel;

    void Awake()
    {
        Assert.IsNotNull(toolBarTabsControl);
        Assert.IsNotNull(vegetationControl);
        Assert.IsNotNull(sunControl);
        Assert.IsNotNull(loadingPanel);
    }

    public void SetUpUI()
    {
        toolBarTabsControl.ActivateDefaultTab();
        vegetationControl.SetUpUI();
        sunControl.SetUpUI();
        DisplayLoadingScreen(true);
    }

    public void DisplayLoadingScreen(bool display)
    {
        loadingPanel.SetActive(display);
    }
}
