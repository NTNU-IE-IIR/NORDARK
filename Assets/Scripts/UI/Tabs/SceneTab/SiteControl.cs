using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class SiteControl : MonoBehaviour
{
    [SerializeField]
    private EnvironmentManager environmentManager;
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private MapManager mapManager;
    [SerializeField]
    private TMP_Dropdown location;
    [SerializeField]
    private TMP_Dropdown mapStyle;
    [SerializeField]
    private Toggle lightInfrastructures;
    [SerializeField]
    private Toggle mapElevation;
    [SerializeField]
    private Toggle mapBuildings;
    [SerializeField]
    private Toggle mapRoads;

    void Awake()
    {
        Assert.IsNotNull(environmentManager);
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(location);
        Assert.IsNotNull(mapStyle);
        Assert.IsNotNull(lightInfrastructures);
        Assert.IsNotNull(mapElevation);
        Assert.IsNotNull(mapBuildings);
        Assert.IsNotNull(mapRoads);
    }

    void Start()
    {
        location.onValueChanged.AddListener(delegate {
            environmentManager.ChangeLocation(location.value);
        });

        mapStyle.onValueChanged.AddListener(delegate {
            mapManager.SetStyle(mapStyle.value);
        });

        lightInfrastructures.onValueChanged.AddListener(delegate {
            lightsManager.ShowLights(lightInfrastructures.isOn);
        });

        mapElevation.onValueChanged.AddListener(delegate {
            mapManager.SetElevationType(mapElevation.isOn);
        });

        mapBuildings.onValueChanged.AddListener(delegate {
            mapManager.DisplayBuildings(mapBuildings.isOn);
        });
        
        mapRoads.onValueChanged.AddListener(delegate {
            mapManager.DisplayRoads(mapRoads.isOn);
        });
    }

    public void AddLocation(string locationName)
    {
        location.AddOptions(new List<string> { locationName });
    }

    public void ChangeLocation(int locationIndex)
    {
        location.value = locationIndex;
    }

    public void ClearLocations()
    {
        location.ClearOptions();
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
