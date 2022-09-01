using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class TabControl : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private Button sceneButton;
    [SerializeField]
    private Button lightButton;
    [SerializeField]
    private Button cameraButton;
    [SerializeField]
    private GameObject sceneTab;
    [SerializeField]
    private GameObject lightTab;
    [SerializeField]
    private GameObject cameraTab;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(sceneButton);
        Assert.IsNotNull(lightButton);
        Assert.IsNotNull(cameraButton);
        Assert.IsNotNull(sceneTab);
        Assert.IsNotNull(lightTab);
        Assert.IsNotNull(cameraTab);
    }

    void Start()
    {
        sceneButton.onClick.AddListener(delegate { ActivateTab(Tab.Scene); });
        lightButton.onClick.AddListener(delegate { ActivateTab(Tab.Light); });
        cameraButton.onClick.AddListener(delegate { ActivateTab(Tab.Camera); });
    }

    public void ActivateDefaultTab()
    {
        ActivateTab(Tab.Scene);
    }

    private void ActivateTab(Tab tab)
    {
        Clear();
        switch (tab) {
            case Tab.Scene:
                sceneTab.SetActive(true);
                break;
            case Tab.Light:
                lightTab.SetActive(true);
                break;
            case Tab.Camera:
                cameraTab.SetActive(true);
                break;
            default:
                break;
        }
    }

    private void Clear()
    {
        sceneTab.SetActive(false);
        lightTab.SetActive(false);
        cameraTab.SetActive(false);

        lightsManager.ClearSelectedLight();
    }

    enum Tab
    {
        Scene,
        Light,
        Camera
    }
}