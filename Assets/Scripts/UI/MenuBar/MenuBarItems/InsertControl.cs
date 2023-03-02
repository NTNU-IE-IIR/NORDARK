using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InsertControl : MenuBarItemControl
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private HeightMapsManager heightMapsManager;
    [SerializeField] private Button lights;
    [SerializeField] private Button groundTextures;
    [SerializeField] private Button heightMaps;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(heightMapsManager);
        Assert.IsNotNull(lights);
        Assert.IsNotNull(groundTextures);
        Assert.IsNotNull(heightMaps);

        base.SetUp();
    }
    
    void Start()
    {
        lights.onClick.AddListener(delegate {
            lightPolesManager.AddLightPolesFromFile();
            gameObject.SetActive(false);
        });

        groundTextures.onClick.AddListener(delegate {
            groundTexturesManager.AddGroundTexturesFromFile();
            gameObject.SetActive(false);
        });

        heightMaps.onClick.AddListener(delegate {
            heightMapsManager.AddHeightMapFromFile();
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
