using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class EnvironmentControl : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private EnvironmentManager environmentManager;
    [SerializeField]
    private Dropdown environment;
    [SerializeField]
    private Dropdown environmentLocation;
    [SerializeField]
    private Toggle lightInfrastructures;
    [SerializeField]
    private GameObject mapControls;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(environmentManager);
        Assert.IsNotNull(environment);
        Assert.IsNotNull(environmentLocation);
        Assert.IsNotNull(lightInfrastructures);
        Assert.IsNotNull(mapControls);
    }

    void Start()
    {
        environment.onValueChanged.AddListener(delegate {
            if (environment.value == 0) {
                mapControls.SetActive(true);
                environmentManager.ShowMap();
            } else {
                mapControls.SetActive(false);
                environmentManager.Show3DScene();
            }
        });

        environmentLocation.onValueChanged.AddListener(delegate {
            environmentManager.ChangeLocation(environmentLocation.value);
        });

        lightInfrastructures.onValueChanged.AddListener(delegate {
            lightsManager.ShowLights(lightInfrastructures.isOn);
        });
    }

    public void AddLocation(string locationName)
    {
        environmentLocation.AddOptions(new List<string> { locationName });
    }
}