using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private SceneManager sceneManager;
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private UIController uIController;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(uIController);
    }

    void Start()
    {
        uIController.SetUpUI();

        StartCoroutine(LoadDefaultScene());
    }

    IEnumerator LoadDefaultScene()
    {
        yield return new WaitUntil(() => mapManager.IsMapInitialized());
        sceneManager.LoadDefaultScene();
        uIController.DisplayLoadingScreen(false);
    }
}
