using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private TabControl tabControl;
    [SerializeField]
    private GameObject loadingPanel;

    void Awake()
    {
        Assert.IsNotNull(tabControl);
        Assert.IsNotNull(loadingPanel);
    }

    public void SetUpUI()
    {
        tabControl.ActivateDefaultTab();
        DisplayLoadingScreen(true);
    }

    public void DisplayLoadingScreen(bool display)
    {
        loadingPanel.SetActive(display);
    }
}
