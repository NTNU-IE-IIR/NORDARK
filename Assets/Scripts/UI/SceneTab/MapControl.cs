using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class MapControl : MonoBehaviour
{
    private static readonly Mapbox.Utils.Vector2d ALESUND_COORDINATES = new Mapbox.Utils.Vector2d(62.4676991855481, 6.30334069538369);
    private static readonly Mapbox.Utils.Vector2d UPPSALA_COORDINATES = new Mapbox.Utils.Vector2d(59.809179, 17.7043457);

    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private Dropdown mapStyle;
    [SerializeField]
    private Dropdown mapLocation;
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

        mapLocation.onValueChanged.AddListener(delegate {
            if (mapLocation.value == 0) {
                mapManager.SetMapLocation(ALESUND_COORDINATES);
            } else {
                mapManager.SetMapLocation(UPPSALA_COORDINATES);
            }
        });

        mapTerrain.onValueChanged.AddListener(delegate
        {
            mapManager.SetElevation(mapTerrain.isOn);
        });

        mapBuildings.onValueChanged.AddListener(delegate {
            mapManager.DisplayBuildings(mapBuildings.isOn);
        });
        
        mapRoads.onValueChanged.AddListener(delegate {
            mapManager.DisplayRoads(mapRoads.isOn);
        });
    }
}
