using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private SceneManager sceneManager;
    [SerializeField]
    private EnvironmentManager environmentManager;
    [SerializeField]
    private UIController uIController;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(environmentManager);
        Assert.IsNotNull(uIController);
    }

    void Start()
    {
        uIController.SetUpUI();
        environmentManager.SetDefaultEnvironment();

        StartCoroutine(LoadDefaultScene());
    }

    IEnumerator LoadDefaultScene()
    {
        yield return new WaitUntil(() => environmentManager.isEnvironmentReady());
        sceneManager.LoadDefaultScene();
        uIController.DisplayLoadingScreen(false);
    }
}
