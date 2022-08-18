using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class SceneControl : MonoBehaviour
{
    [SerializeField]
    private SceneManager sceneManager;
    [SerializeField]
    private Button load;
    [SerializeField]
    private Button save;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(load);
        Assert.IsNotNull(save);
    }
    
    void Start()
    {
        load.onClick.AddListener(delegate { sceneManager.Load(); });
        save.onClick.AddListener(delegate { sceneManager.Save(); });
    }
}
