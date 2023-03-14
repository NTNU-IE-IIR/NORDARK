using UnityEngine;
using UnityEngine.Assertions;

public class SkyManager : MonoBehaviour
{
    private const float MOON_MAX_INTENSITY = 0.5f;
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

        Coordinate coordinate = new Coordinate();
        Location currentLocation = locationsManager.GetCurrentLocation();
        if (currentLocation != null) {
            coordinate = currentLocation.Coordinate;
        }

        Sampa.Sampa sampa = new Sampa.Sampa();
        sampa.Calculate(_dateTime, coordinate);
        
        sun.localEulerAngles = new Vector3((float) sampa.spa.e, (float) sampa.spa.azimuth, 0);
        moon.localEulerAngles = new Vector3((float) sampa.mpa.e, (float) sampa.mpa.azimuth, 0);
    }

    private void SetMoonIntensity()
    {
        moonLight.SetIntensity(Mathf.Lerp(0, MOON_MAX_INTENSITY, (float) MoonPhaseConsole.Moon.Calculate(_dateTime).Visibility/100));
    }
}
