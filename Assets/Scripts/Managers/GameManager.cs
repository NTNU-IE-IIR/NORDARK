using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public const string RESOURCES_FOLDER_NAME = "AdditionnalResources";
    private const string FIRST_LAUNCH_KEY = "FIRST_LAUNCH";
    [SerializeField] private GroundTextureMasksManager groundTextureMasksManager;
    [SerializeField] private IESManager iESManager;
    [SerializeField] private HeightMapsManager heightMapsManager;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private UIController uIController;

    void Awake()
    {
        Assert.IsNotNull(groundTextureMasksManager);
        Assert.IsNotNull(iESManager);
        Assert.IsNotNull(heightMapsManager);
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(uIController);

        if (IsFirstLaunch()) {
            groundTextureMasksManager.AddMasksFromResources();
            iESManager.AddIESFilesFromResources();
            heightMapsManager.AddHeightMapsFromResources();
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
