using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private ToolBarTabsControl toolBarTabsControl;
    [SerializeField] private VegetationControl vegetationControl;
    [SerializeField] private SkyControl skyControl;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text version;

    void Awake()
    {
        Assert.IsNotNull(toolBarTabsControl);
        Assert.IsNotNull(vegetationControl);
        Assert.IsNotNull(skyControl);
        Assert.IsNotNull(version);
    }

    public void SetUpUI()
    {
        version.text = Application.version;
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
