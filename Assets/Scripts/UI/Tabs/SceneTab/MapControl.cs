using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class MapControl : MonoBehaviour
{
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private Dropdown mapStyle;
    [SerializeField]
    private Toggle mapTerrain;
    [SerializeField]
    private Toggle mapBuildings;
    [SerializeField]
    private Toggle mapRoads;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(mapStyle);
        Assert.IsNotNull(mapTerrain);
        Assert.IsNotNull(mapBuildings);
        Assert.IsNotNull(mapRoads);
    }

    void Start()
    {
        mapStyle.onValueChanged.AddListener(delegate {
            mapManager.SetStyle(mapStyle.value);
        });

        mapTerrain.onValueChanged.AddListener(delegate {
            mapManager.SetElevationType(mapTerrain.isOn);
        });

        mapBuildings.onValueChanged.AddListener(delegate {
            mapManager.DisplayBuildings(mapBuildings.isOn);
        });
        
        mapRoads.onValueChanged.AddListener(delegate {
            mapManager.DisplayRoads(mapRoads.isOn);
        });
    }
}