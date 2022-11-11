using UnityEngine;
using UnityEngine.Assertions;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private ToolBarTabsControl toolBarTabsControl;
    [SerializeField]
    private VegetationControl vegetationControl;
    [SerializeField]
    private SkyControl skyControl;
    [SerializeField]
    private GameObject loadingPanel;

    void Awake()
    {
        Assert.IsNotNull(toolBarTabsControl);
        Assert.IsNotNull(vegetationControl);
        Assert.IsNotNull(skyControl);
        Assert.IsNotNull(loadingPanel);
    }

    public void SetUpUI()
    {
        toolBarTabsControl.ActivateDefaultTab();
        vegetationControl.SetUpUI();
        skyControl.SetUpUI();
        DisplayLoadingScreen(true);
    }

    public void DisplayLoadingScreen(bool display)
    {
        loadingPanel.SetActive(display);
    }
}
