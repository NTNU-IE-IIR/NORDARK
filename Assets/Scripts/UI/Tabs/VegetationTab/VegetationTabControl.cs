using UnityEngine;
using UnityEngine.Assertions;

public class VegetationTabControl : TabControl
{
    [SerializeField] private VegetationObjectsManager vegetationObjectsManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private GameObject biomeAreasControl;

    void Awake()
    {
        Assert.IsNotNull(vegetationObjectsManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(biomeAreasControl);
    }
    public override void OnTabOpened()
    {
        // Biome areas are only available for the map terrain type
        Location currentLocation = locationsManager.GetCurrentLocation();
        biomeAreasControl.SetActive(currentLocation != null && currentLocation.Type == Location.TerrainType.Map);
    }

    public override void OnTabClosed()
    {
        vegetationObjectsManager.ClearSelectedObjects();
    }
}