using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class VegetationControl : MonoBehaviour
{
    [SerializeField] private VegetationManager vegetationManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private Toggle displayVegetation;
    [SerializeField] private TMP_Dropdown biomeAreas;
    [SerializeField] private Button addBiomeArea;
    [SerializeField] private Button deleteBiomeArea;
    [SerializeField] private TMP_Dropdown biomes;
    [SerializeField] private Button addNode;
    [SerializeField] private Button deleteNode;
    [SerializeField] private Toggle displayNodes;
    [SerializeField] private GameObject selectionPin;
    [SerializeField] private Transform selectionPinContainer;
    private SelectionPin movingPin;

    void Awake()
    {
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(displayVegetation);
        Assert.IsNotNull(biomeAreas);
        Assert.IsNotNull(addBiomeArea);
        Assert.IsNotNull(deleteBiomeArea);
        Assert.IsNotNull(biomes);
        Assert.IsNotNull(addNode);
        Assert.IsNotNull(deleteNode);
        Assert.IsNotNull(displayNodes);
        Assert.IsNotNull(selectionPin);
        Assert.IsNotNull(selectionPinContainer);

        movingPin = null;
    }

    void Start()
    {
        displayVegetation.onValueChanged.AddListener(change => vegetationManager.GenerateBiomes());
        biomeAreas.onValueChanged.AddListener(change => BiomeAreaChanged());
        addBiomeArea.onClick.AddListener(AddBiomeArea);
        deleteBiomeArea.onClick.AddListener(DeleteBiomeArea);
        biomes.onValueChanged.AddListener(change => vegetationManager.ChangeBiome(biomes.options[biomes.value].text));
        addNode.onClick.AddListener(AddNode);
        deleteNode.onClick.AddListener(DeleteNode);
        displayNodes.onValueChanged.AddListener(change => DisplayNodes());
    }

    void Update()
    {
        if (movingPin != null && Input.GetMouseButtonDown(0)) {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (!isOverUI) {
                vegetationManager.AddNodeToCurrentBiomeArea(movingPin.gameObject.transform.position);
                DisplayNodes();
            }
            
            movingPin.gameObject.Destroy();
            movingPin = null;
        }
    }

    public void SetUpUI()
    {
        biomes.AddOptions(vegetationManager.GetBiomes());
    }

    public bool isVegetationDisplayed()
    {
        return displayVegetation.isOn;
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
            movingPin.gameObject.Destroy();
            movingPin = null;
        }
        
        displayNodes.isOn = false;
        DisplayNodes();
    }

    public int GetBiomeAreaIndex()
    {
        return biomeAreas.value;
    }

    private void BiomeAreaChanged()
    {
        UpdateBiome();
        DisplayNodes();
    }

    private void AddBiomeArea()
    {
        EventSystem.current.SetSelectedGameObject(null);

        BiomeArea biomeArea = new BiomeArea();
        biomeArea.Biome = biomes.options[biomes.value].text;
        vegetationManager.AddBiomeArea(biomeArea);
        biomeAreas.value = biomeAreas.options.Count - 1;
        BiomeAreaChanged();
    }

    private void DeleteBiomeArea()
    {
        EventSystem.current.SetSelectedGameObject(null);

        vegetationManager.DeleteSelectedBiomeArea();
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
        movingPin.SetMoving(true);
    }

    private void DeleteNode()
    {
        EventSystem.current.SetSelectedGameObject(null);
        vegetationManager.DeleteLastNodeOfCurrentBiomeArea();
        DisplayNodes();
    }

    private void DisplayNodes()
    {
        foreach (Transform child in selectionPinContainer) {
            GameObject.Destroy(child.gameObject);
        }
        if (displayNodes.isOn) {
            List<Vector2d> nodeCoordinates = vegetationManager.GetCoordinateOfCurrentBiomeArea();
            foreach(Vector2d coordinate in nodeCoordinates) {
                Instantiate(selectionPin, mapManager.GetUnityPositionFromCoordinatesAndAltitude(coordinate, 0, true), Quaternion.Euler(-90, 0, 0), selectionPinContainer);
            }
        }
    }

    private void UpdateBiome()
    {
        string biome = vegetationManager.GetBiomeOfCurrentBiomeArea();
        for (int i=0; i<biomes.options.Count; i++) {
            if (biomes.options[i].text == biome) {
                biomes.value = i;
            }
        }
    }
}
