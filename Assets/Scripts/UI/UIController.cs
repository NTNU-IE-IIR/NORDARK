using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private ToolBarTabsControl toolBarTabsControl;
    [SerializeField] private BiomeAreasControl biomeAreasControl;
    [SerializeField] private SkyControl skyControl;
    [SerializeField] private TMP_Text version;
    [SerializeField] private GameObject loadingPanel;

    void Awake()
    {
        Assert.IsNotNull(toolBarTabsControl);
        Assert.IsNotNull(biomeAreasControl);
        Assert.IsNotNull(skyControl);
        Assert.IsNotNull(version);
        Assert.IsNotNull(loadingPanel);
    }

    public void SetUpUI()
    {
        version.text = Application.version;
        toolBarTabsControl.ActivateDefaultTab();
        biomeAreasControl.SetUpUI();
        skyControl.SetUpUI();
    }

    public void DisplayLoadingPanel(bool display)
    {
        loadingPanel.SetActive(display);
    }
}
