using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class BiomeAreasControl : MonoBehaviour
{
    [SerializeField] private BiomeAreasManager biomeAreasManager;
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private Toggle displayBiomeAreas;
    [SerializeField] private Toggle displayNodes;
    [SerializeField] private TMP_Dropdown biomeAreas;
    [SerializeField] private Button addBiomeArea;
    [SerializeField] private Button deleteBiomeArea;
    [SerializeField] private Button addNode;
    [SerializeField] private Button deleteNode;
    [SerializeField] private Slider density;
    [SerializeField] private TMP_Text densityValue;
    [SerializeField] private TMP_Dropdown biomes;
    [SerializeField] private GameObject selectionPin;
    [SerializeField] private Transform selectionPinContainer;
    private SelectionPin movingPin;

    void Awake()
    {
        Assert.IsNotNull(biomeAreasManager);
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(displayBiomeAreas);
        Assert.IsNotNull(displayNodes);
        Assert.IsNotNull(biomeAreas);
        Assert.IsNotNull(addBiomeArea);
        Assert.IsNotNull(deleteBiomeArea);
        Assert.IsNotNull(addNode);
        Assert.IsNotNull(deleteNode);
        Assert.IsNotNull(density);
        Assert.IsNotNull(densityValue);
        Assert.IsNotNull(biomes);
        Assert.IsNotNull(selectionPin);
        Assert.IsNotNull(selectionPinContainer);

        movingPin = null;
    }

    void Start()
    {
        displayBiomeAreas.onValueChanged.AddListener(change => biomeAreasManager.GenerateBiomes());
        displayNodes.onValueChanged.AddListener(change => DisplayNodes());
        biomeAreas.onValueChanged.AddListener(change => BiomeAreaChanged());
        addBiomeArea.onClick.AddListener(AddBiomeArea);
        deleteBiomeArea.onClick.AddListener(DeleteBiomeArea);
        addNode.onClick.AddListener(AddNode);
        deleteNode.onClick.AddListener(DeleteNode);
        density.onValueChanged.AddListener(change => {
            SetDensity(change);
            biomeAreasManager.ChangeBiomeDensity(change);
        });
        biomes.onValueChanged.AddListener(change => {
            biomeAreasManager.ChangeBiome(biomes.options[change].text);
            SetDensity(biomeAreasManager.GetBiomeDensity(biomes.options[change].text));
        });
    }

    void Update()
    {
        if (movingPin != null && Input.GetMouseButtonDown(0)) {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (!isOverUI) {
                biomeAreasManager.AddNodeToCurrentBiomeArea(movingPin.GetPosition());
                DisplayNodes();
            }
            
            movingPin.Destroy();
            movingPin = null;
        }
    }

    public void SetUpUI()
    {
        biomes.AddOptions(biomeAreasManager.GetBiomes());
    }

    public bool isVegetationDisplayed()
    {
        return displayBiomeAreas.isOn;
    }

    public void AddBiomeAreas(string biomeArea)
    {
        biomeAreas.AddOptions(new List<string> {biomeArea});

        if (biomeAreas.options.Count == 1) {
            BiomeAreaChanged();
        }
    }

    public void ClearBiomeAreas()
    {
        biomeAreas.ClearOptions();

        if (movingPin != null) {
            movingPin.Destroy();
            movingPin = null;
        }
        
        displayNodes.isOn = false;
        DisplayNodes();
    }

    public int GetBiomeAreaIndex()
    {
        return biomeAreas.value;
    }

    public string GetCurrentBiomeName()
    {
        return biomes.options[biomes.value].text;
    }

    private void BiomeAreaChanged()
    {
        UpdateBiome();
        DisplayNodes();
    }

    private void AddBiomeArea()
    {
        EventSystem.current.SetSelectedGameObject(null);

        BiomeArea biomeArea = new BiomeArea(locationsManager.GetCurrentLocation());
        biomeArea.Biome = biomes.options[biomes.value].text;
        biomeAreasManager.AddBiomeArea(biomeArea);
        biomeAreasManager.GenerateBiomes();
        biomeAreas.value = biomeAreas.options.Count - 1;
        BiomeAreaChanged();
    }

    private void DeleteBiomeArea()
    {
        EventSystem.current.SetSelectedGameObject(null);

        biomeAreasManager.DeleteSelectedBiomeArea();
        biomeAreas.options.RemoveAt(biomeAreas.value);
        if (biomeAreas.options.Count < 1) {
            biomeAreas.captionText.text = "";
        } else {
            biomeAreas.value = 0;
        }
        BiomeAreaChanged();
    }

    private void AddNode()
    {
        movingPin = Instantiate(selectionPin, new Vector3(0, 0, 0), Quaternion.Euler(-90, 0, 0)).GetComponent<SelectionPin>();
        movingPin.Create(sceneCamerasManager);
        movingPin.SetMoving(true);
    }

    private void DeleteNode()
    {
        EventSystem.current.SetSelectedGameObject(null);
        biomeAreasManager.DeleteLastNodeOfCurrentBiomeArea();
        DisplayNodes();
    }

    private void DisplayNodes()
    {
        foreach (Transform child in selectionPinContainer) {
            GameObject.Destroy(child.gameObject);
        }
        if (displayNodes.isOn) {
            List<Coordinate> nodeCoordinates = biomeAreasManager.GetCoordinateOfCurrentBiomeArea();
            foreach(Coordinate coordinate in nodeCoordinates) {
                Instantiate(selectionPin, terrainManager.GetUnityPositionFromCoordinates(coordinate, true), Quaternion.Euler(-90, 0, 0), selectionPinContainer);
            }
        }
    }

    private void SetDensity(float newDensity)
    {
        density.SetValueWithoutNotify(newDensity);
        densityValue.text = newDensity.ToString("0.00");
    }

    private void UpdateBiome()
    {
        string biome = biomeAreasManager.GetBiomeOfCurrentBiomeArea();
        for (int i=0; i<biomes.options.Count; i++) {
            if (biomes.options[i].text == biome) {
                biomes.value = i;
                SetDensity(biomeAreasManager.GetBiomeDensity(biome));
            }
        }
    }
}
