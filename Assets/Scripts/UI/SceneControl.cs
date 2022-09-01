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
    [SerializeField]
    private Button quit;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(load);
        Assert.IsNotNull(save);
        Assert.IsNotNull(quit);
    }
    
    void Start()
    {
        load.onClick.AddListener(delegate { sceneManager.Load(); });
        save.onClick.AddListener(delegate { sceneManager.Save(); });
        quit.onClick.AddListener(delegate { Application.Quit(); });
    }
}
