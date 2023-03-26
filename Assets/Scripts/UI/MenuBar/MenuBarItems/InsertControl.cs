using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class InsertControl : MenuBarItemControl
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private HeightMapsManager heightMapsManager;
    [SerializeField] private Button lightsGeoJSON;
    [SerializeField] private Button lightsCSV;
    [SerializeField] private Button groundTextures;
    [SerializeField] private Button heightMaps;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(heightMapsManager);
        Assert.IsNotNull(lightsGeoJSON);
        Assert.IsNotNull(lightsCSV);
        Assert.IsNotNull(groundTextures);
        Assert.IsNotNull(heightMaps);

        base.SetUp();
    }
    
    void Start()
    {
        lightsGeoJSON.onClick.AddListener(delegate {
            lightPolesManager.AddLightPolesFromGeoJSONFile();
            gameObject.SetActive(false);
        });
        
        lightsCSV.onClick.AddListener(delegate {
            lightPolesManager.AddLightPolesFromCSVFile();
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
