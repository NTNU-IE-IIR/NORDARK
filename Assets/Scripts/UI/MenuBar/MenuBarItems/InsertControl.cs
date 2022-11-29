using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InsertControl : MenuBarItemControl
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private Button lights;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(lights);

        base.SetUp();
    }
    
    void Start()
    {
        lights.onClick.AddListener(delegate {
            sceneManager.AddLights();
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
