using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class TabControl : MonoBehaviour
{
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

    private GameObject[] tabs;

    void Awake()
    {
        Assert.IsNotNull(sceneButton);
        Assert.IsNotNull(lightButton);
        Assert.IsNotNull(cameraButton);
        Assert.IsNotNull(sceneTab);
        Assert.IsNotNull(lightTab);
        Assert.IsNotNull(cameraTab);

        tabs = new GameObject[3];
        tabs[0] = sceneTab;
        tabs[1] = lightTab;
        tabs[2] = cameraTab;
    }

    void Start()
    {
        sceneButton.onClick.AddListener(delegate { ActivateTab(0); });
        lightButton.onClick.AddListener(delegate { ActivateTab(1); });
        cameraButton.onClick.AddListener(delegate { ActivateTab(2); });
    }

    public void ActivateDefaultTab()
    {
        ActivateTab(0);
    }

    private void ActivateTab(int tabIndex = 0)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(false);
        }
        tabs[tabIndex].SetActive(true);
    }
}