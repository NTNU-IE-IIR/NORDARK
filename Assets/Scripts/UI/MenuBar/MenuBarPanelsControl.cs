using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class MenuBarPanelsControl : MonoBehaviour
{
    [SerializeField]
    private Button file;
    [SerializeField]
    private Button upload;
    [SerializeField]
    private Button view;
    [SerializeField]
    private Button help;
    [SerializeField]
    private GameObject filePanel;
    [SerializeField]
    private GameObject uploadPanel;
    [SerializeField]
    private GameObject viewPanel;
    [SerializeField]
    private GameObject helpPanel;


    void Awake()
    {
        Assert.IsNotNull(file);
        Assert.IsNotNull(upload);
        Assert.IsNotNull(view);
        Assert.IsNotNull(help);
        Assert.IsNotNull(filePanel);
        Assert.IsNotNull(uploadPanel);
        Assert.IsNotNull(viewPanel);
        Assert.IsNotNull(helpPanel);
    }

    void Start()
    {
        file.onClick.AddListener(delegate { OpenPanel(Panel.File); });
        upload.onClick.AddListener(delegate { OpenPanel(Panel.Upload); });
        view.onClick.AddListener(delegate { OpenPanel(Panel.View); });
        help.onClick.AddListener(delegate { OpenPanel(Panel.Help); });
    }

    public void OnFilePanelHovered()
    {
        if (OnePanelOpened()) {
            OpenPanel(Panel.File);
        }
    }

    public void OnUploadPanelHovered()
    {
        if (OnePanelOpened()) {
            OpenPanel(Panel.Upload);
        }
    }

    public void OnViewPanelHovered()
    {
        if (OnePanelOpened()) {
            OpenPanel(Panel.View);
        }
    }

    public void OnHelpPanelHovered()
    {
        if (OnePanelOpened()) {
            OpenPanel(Panel.Help);
        }
    }

    private void OpenPanel(Panel panel)
    {
        ClearPanels();
        switch (panel) {
            case Panel.File:
                filePanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(file.gameObject);
                break;
            case Panel.Upload:
                uploadPanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(upload.gameObject);
                break;
            case Panel.View:
                viewPanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(view.gameObject);
                break;
            case Panel.Help:
                helpPanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(help.gameObject);
                break;
        }
    }

    private bool OnePanelOpened()
    {
        return filePanel.activeSelf || uploadPanel.activeSelf || viewPanel.activeSelf || helpPanel.activeSelf;
    }

    private void ClearPanels()
    {
        filePanel.SetActive(false);
        uploadPanel.SetActive(false);
        viewPanel.SetActive(false);
        helpPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    enum Panel
    {
        File,
        Upload,
        View,
        Help
    }
}
