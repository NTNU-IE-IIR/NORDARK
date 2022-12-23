using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InsertControl : MenuBarItemControl
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private Button lights;
    [SerializeField] private Button groundTextures;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(lights);
        Assert.IsNotNull(groundTextures);

        base.SetUp();
    }
    
    void Start()
    {
        lights.onClick.AddListener(delegate {
            sceneManager.AddLights();
            gameObject.SetActive(false);
        });

        groundTextures.onClick.AddListener(delegate {
            sceneManager.AddGroundTextures();
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
