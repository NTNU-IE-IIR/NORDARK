using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ToolBarTabsControl : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private StreetViewManager streetViewManager;
    [SerializeField]
    private Button sceneButton;
    [SerializeField]
    private Button lightButton;
    [SerializeField]
    private Button cameraButton;
    [SerializeField]
    private Button streetViewButton;
    [SerializeField]
    private GameObject sceneTab;
    [SerializeField]
    private GameObject lightTab;
    [SerializeField]
    private GameObject cameraTab;
    [SerializeField]
    private GameObject streetViewTab;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(streetViewManager);
        Assert.IsNotNull(sceneButton);
        Assert.IsNotNull(lightButton);
        Assert.IsNotNull(cameraButton);
        Assert.IsNotNull(streetViewButton);
        Assert.IsNotNull(sceneTab);
        Assert.IsNotNull(lightTab);
        Assert.IsNotNull(cameraTab);
        Assert.IsNotNull(streetViewTab);
    }

    void Start()
    {
        sceneButton.onClick.AddListener(delegate { ActivateTab(Tab.Scene); });
        lightButton.onClick.AddListener(delegate { ActivateTab(Tab.Light); });
        cameraButton.onClick.AddListener(delegate { ActivateTab(Tab.Camera); });
        streetViewButton.onClick.AddListener(delegate { ActivateTab(Tab.StreetView); });
    }

    public void ActivateDefaultTab()
    {
        ActivateTab(Tab.Scene);
    }

    private void ActivateTab(Tab tab)
    {
        GameObject tabObject = null;
        Button button = null;

        Clear();
        switch (tab) {
            case Tab.Scene:
                tabObject = sceneTab;
                button = sceneButton;
                break;
            case Tab.Light:
                tabObject = lightTab;
                button = lightButton;
                break;
            case Tab.Camera:
                tabObject = cameraTab;
                button = cameraButton;
                camerasManager.DisplayCameraPreview(true);
                break;
            case Tab.StreetView:
                tabObject = streetViewTab;
                button = streetViewButton;
                streetViewManager.DisplayCameraPreview(true);
                break;
            default:
                break;
        }

        if (tabObject != null) {
            tabObject.SetActive(true);
            ColorBlock cb = button.colors;
            cb.normalColor = Color.black;
            button.colors = cb;
        }
    }

    private void Clear()
    {
        sceneTab.SetActive(false);
        lightTab.SetActive(false);
        cameraTab.SetActive(false);
        streetViewTab.SetActive(false);

        ColorBlock cbScene = sceneButton.colors;
        cbScene.normalColor = Color.clear;
        sceneButton.colors = cbScene;
        
        ColorBlock cbLight = lightButton.colors;
        cbLight.normalColor = Color.clear;
        lightButton.colors = cbLight;
       
        ColorBlock cbCamera = cameraButton.colors;
        cbCamera.normalColor = Color.clear;
        cameraButton.colors = cbCamera;
       
        ColorBlock cbStreetView = streetViewButton.colors;
        cbStreetView.normalColor = Color.clear;
        streetViewButton.colors = cbStreetView;

        lightsManager.ClearSelectedLight();
        camerasManager.DisplayCameraPreview(false);
        streetViewManager.DisplayCameraPreview(false);
    }

    enum Tab
    {
        Scene,
        Light,
        Camera,
        StreetView
    }
}