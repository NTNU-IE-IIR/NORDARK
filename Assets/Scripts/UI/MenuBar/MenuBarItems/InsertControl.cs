using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InsertControl : MenuBarItemControl
{
    [SerializeField] private LightsManager lightsManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private Button lights;
    [SerializeField] private Button groundTextures;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(lights);
        Assert.IsNotNull(groundTextures);

        base.SetUp();
    }
    
    void Start()
    {
        lights.onClick.AddListener(delegate {
            lightsManager.AddLightsFromFile();
            gameObject.SetActive(false);
        });

        groundTextures.onClick.AddListener(delegate {
            groundTexturesManager.AddGroundTexturesFromFile();
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
