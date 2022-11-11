using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class LocationControl : MonoBehaviour
{
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private TMP_Dropdown location;

    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(location);
    }

    void Start()
    {
        location.onValueChanged.AddListener(locationsManager.ChangeLocation);
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
}
