using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SkyManager : MonoBehaviour
{
    private const float MOON_MAX_INTENSITY = 0.5f;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private Transform sun;
    [SerializeField] private Transform moon;
    private System.DateTime _dateTime;
    private UnityEngine.Rendering.HighDefinition.HDAdditionalLightData moonLight;
    public System.DateTime DateTime
    {
        get => _dateTime;
        set
        {
            _dateTime = value;
            UpdateSunAndMoon();
        }
    }

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(sun);
        Assert.IsNotNull(moon);

        _dateTime = System.DateTime.Now;
        moonLight = moon.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
    }

    public void OnLocationChanged()
    {
        UpdateSunAndMoon();
    }

    private void UpdateSunAndMoon()
    {
        SetSunAndMoonPosition();
        SetMoonIntensity();
    }

    private void SetSunAndMoonPosition()
    {

        Vector3d coordinates = new Vector3d();
        Location currentLocation = locationsManager.GetCurrentLocation();
        if (currentLocation != null) {
            coordinates = currentLocation.Coordinates;
        }

        Sampa.Sampa sampa = new Sampa.Sampa();
        sampa.Calculate(_dateTime, coordinates);
        
        sun.localEulerAngles = new Vector3((float) sampa.spa.e, (float) sampa.spa.azimuth, 0);
        moon.localEulerAngles = new Vector3((float) sampa.mpa.e, (float) sampa.mpa.azimuth, 0);
    }

    private void SetMoonIntensity()
    {
        moonLight.SetIntensity(Mathf.Lerp(0, MOON_MAX_INTENSITY, (float) MoonPhaseConsole.Moon.Calculate(_dateTime).Visibility/100));
    }
}
