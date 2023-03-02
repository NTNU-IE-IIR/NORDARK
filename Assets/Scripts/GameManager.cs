using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public const string RESOURCES_FOLDER_NAME = "AdditionnalResources";
    private string FIRST_LAUNCH_KEY = "FIRST_LAUNCH";
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private IESManager iESManager;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UIController uIController;

    void Awake()
    {
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(iESManager);
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(uIController);

        if (IsFirstLaunch()) {
            groundTexturesManager.AddMasksFromResources();
            iESManager.AddIESFilesFromResources();
        }
    }

    void Start()
    {
        uIController.SetUpUI();

        StartCoroutine(LoadDefaultScene());
    }

    private bool IsFirstLaunch()
    {
        string key = FIRST_LAUNCH_KEY + Application.version;
        bool firstLaunch = !PlayerPrefs.HasKey(key);

        if (firstLaunch) {
            PlayerPrefs.SetInt(key, 1);
        }
        
        return firstLaunch;
    }

    private IEnumerator LoadDefaultScene()
    {
        yield return new WaitUntil(() => mapManager.IsMapInitialized());
        sceneManager.LoadDefaultScene();
        uIController.DisplayLoadingScreen(false);
    }
}
