using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class IESControl : MonoBehaviour
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
        upload.onClick.AddListener(delegate {
            iesManager.Upload();
            EventSystem.current.SetSelectedGameObject(null);
        });
    }
}