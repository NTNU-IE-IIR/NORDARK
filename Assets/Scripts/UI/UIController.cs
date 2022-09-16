using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private ToolBarTabsControl toolBarTabsControl;
    [SerializeField]
    private GameObject loadingPanel;

    void Awake()
    {
        Assert.IsNotNull(toolBarTabsControl);
        Assert.IsNotNull(loadingPanel);
    }

    public void SetUpUI()
    {
        toolBarTabsControl.ActivateDefaultTab();
        DisplayLoadingScreen(true);
    }

    public void DisplayLoadingScreen(bool display)
    {
        loadingPanel.SetActive(display);
    }
}
