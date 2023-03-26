using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class LocationControl : MonoBehaviour
{
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private NewLocationWindow newLocationWindow;
    [SerializeField] private TMP_Dropdown location;
    [SerializeField] private Button createLocation;
    [SerializeField] private Button deleteLocation;

    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(newLocationWindow);
        Assert.IsNotNull(location);
        Assert.IsNotNull(createLocation);
        Assert.IsNotNull(deleteLocation);
    }

    void Start()
    {
        location.onValueChanged.AddListener(value => {
            if (sceneManager.AreThereChangesUnsaved()) {
                Location currentLocation = locationsManager.GetCurrentLocation();
                if (currentLocation == null) {
                    location.SetValueWithoutNotify(-1);
                } else {
                    location.SetValueWithoutNotify(location.options.FindIndex(option => option.text == currentLocation.Name));
                }

                DialogControl.CreateConfirmationDialog("Changes are not saved. Continue?", () => {
                    locationsManager.ChangeLocation(value);
                });
            } else {
                locationsManager.ChangeLocation(value);
            }
        });

        createLocation.onClick.AddListener(CreateLocation);
        deleteLocation.onClick.AddListener(DeleteCurrentLocation);
    }

    public void AddLocation(string locationName)
    {
        location.AddOptions(new List<string> { locationName });
    }

    public void ChangeLocation(int locationIndex)
    {
        location.SetValueWithoutNotify(locationIndex);
    }

    public void ClearLocations()
    {
        location.ClearOptions();
    }

    private void CreateLocation()
    {
        newLocationWindow.Open();
    }

    private void DeleteCurrentLocation()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (location.options.Count > 0) {
            int locationIndex = location.value;

            List<string> newLocations = new List<string>();

            foreach (TMP_Dropdown.OptionData option in location.options) {
                if (option.text != location.options[locationIndex].text) {
                    newLocations.Add(option.text);
                }
            }
            location.ClearOptions();
            location.AddOptions(newLocations);

            locationsManager.DeleteCurrentLocation(locationIndex);
        }
    }
}
