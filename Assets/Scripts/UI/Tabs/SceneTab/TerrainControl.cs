using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class TerrainControl : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private TMP_Dropdown ground;
    [SerializeField] private Toggle buildings;

    void Awake()
    {
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(ground);
        Assert.IsNotNull(buildings);
    }

    void Start()
    {
        ground.AddOptions(groundTexturesManager.GetTextureNames());

        ground.onValueChanged.AddListener(terrainManager.SetStyle);
        buildings.onValueChanged.AddListener(terrainManager.DisplayBuildings);
    }

    public bool IsBuildingLayerActive()
    {
        return buildings.isOn;
    }

    public string GetGround()
    {
        return ground.options[ground.value].text;
    }

    public int GetGroundIndex()
    {
        return ground.value;
    }
}
