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

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(mapStyle);
        Assert.IsNotNull(mapElevation);
        Assert.IsNotNull(mapBuildings);
    }

    void Start()
    {
        mapStyle.onValueChanged.AddListener(mapManager.SetStyle);
        mapElevation.onValueChanged.AddListener(mapManager.SetElevationType);
        mapBuildings.onValueChanged.AddListener(mapManager.DisplayBuildings);
    }

    public bool IsBuildingLayerActive()
    {
        return mapBuildings.isOn;
    }
}
