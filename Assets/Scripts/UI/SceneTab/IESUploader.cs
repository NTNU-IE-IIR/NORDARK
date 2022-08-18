using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using SFB;

public class IESUploader : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private Button upload;
    [SerializeField]
    private Dropdown IESList;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(upload);
        Assert.IsNotNull(IESList);
    }

    void Start()
    {
        upload.onClick.AddListener(OpenUploadWindow);

        IESList.AddOptions(lightsManager.GetIESNames());
        IESList.onValueChanged.AddListener(delegate {
            lightsManager.SetIESToAllLights(IESList.options[IESList.value].text);
        });
    }

// TODO: voir ce qu'on fait lors d'un upload de IES
    private void OpenUploadWindow()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Upload a IES file", "", "ies", true);
        for (int i = 0; i < paths.Length; i++) {
            UnityEditor.FileUtil.MoveFileOrDirectory(paths[i], "Assets/IES folder/");
        }
    }
}
