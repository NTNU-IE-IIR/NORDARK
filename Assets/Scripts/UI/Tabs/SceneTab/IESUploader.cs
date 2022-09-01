using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using SFB;

public class IESUploader : MonoBehaviour
{
    [SerializeField]
    private IESManager iesManager;
    [SerializeField]
    private Button upload;

    void Awake()
    {
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(upload);
    }

    void Start()
    {
        upload.onClick.AddListener(OpenUploadWindow);
    }

    private void OpenUploadWindow()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Upload a IES file", "", "ies", true);
        for (int i = 0; i < paths.Length; i++) {
            iesManager.AddIES(paths[i]);
        }
    }
}