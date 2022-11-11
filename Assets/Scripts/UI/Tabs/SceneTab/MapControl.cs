using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class MapControl : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private TMP_Dropdown mapStyle;
    [SerializeField] private Toggle mapElevation;
    [SerializeField] private Toggle mapBuildings;
    [SerializeField] private Toggle mapRoads;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(mapStyle);
        Assert.IsNotNull(mapElevation);
        Assert.IsNotNull(mapBuildings);
        Assert.IsNotNull(mapRoads);
    }

    void Start()
    {
        mapStyle.onValueChanged.AddListener(mapManager.SetStyle);
        mapElevation.onValueChanged.AddListener(mapManager.SetElevationType);
        mapBuildings.onValueChanged.AddListener(mapManager.DisplayBuildings);
        mapRoads.onValueChanged.AddListener(mapManager.DisplayRoads);
    }

    public bool IsBuildingLayerActive()
    {
        return mapBuildings.isOn;
    }

    public bool IsRoadLayerActive()
    {
        return mapRoads.isOn;
    }
}
