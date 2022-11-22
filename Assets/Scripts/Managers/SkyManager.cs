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
    private System.DateTime sceneDateTime;
    private List<Sampa.Timescale> timescales;
    private UnityEngine.Rendering.HighDefinition.HDAdditionalLightData moonLight;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(sun);
        Assert.IsNotNull(moon);

        sceneDateTime = System.DateTime.Now;
        timescales = JsonUtility.FromJson<Sampa.Timescales>(Resources.Load<TextAsset>("Timescales").text).timescales;
        moonLight = moon.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
    }

    public System.DateTime GetCurrentDateTime()
    {
        return sceneDateTime;
    }

    public void SetCurrentDateTime(System.DateTime newDateTime)
    {
        sceneDateTime = newDateTime;
        UpdateSunAndMoon();
    }

    public void OnLocationChanged()
    {
        UpdateSunAndMoon();
    }

    private void UpdateSunAndMoon()
    {
        int i=0;
        while (i < timescales.Count-1 && sceneDateTime > timescales[i].dateTime) {
            i++;
        }
        Sampa.Timescale timescale = timescales[i];

        Vector3d coordinates = new Vector3d();
        Location currentLocation = locationsManager.GetCurrentLocation();
        if (currentLocation != null) {
            coordinates = currentLocation.Coordinates;
        }

        Sampa.Sampa sampa = new Sampa.Sampa();
        sampa.year = sceneDateTime.Year;
        sampa.month = sceneDateTime.Month;
        sampa.day = sceneDateTime.Day;
        sampa.hour = sceneDateTime.Hour;
        sampa.minute = sceneDateTime.Minute;
        sampa.second = sceneDateTime.Second;
        sampa.timezone = System.TimeZoneInfo.Local.BaseUtcOffset.Hours;
        sampa.delta_ut1 = timescale.delta_ut1;
        sampa.delta_t = timescale.delta_t;
        sampa.longitude = coordinates.y;
        sampa.latitude = coordinates.x;
        sampa.elevation = coordinates.altitude;
        sampa.pressure = 1000;
        sampa.temperature = 11;
        sampa.atmos_refract = 0.5667;
        sampa.Calculate();
        
        sun.localEulerAngles = new Vector3((float) sampa.spa.e, (float) sampa.spa.azimuth, 0);
        moon.localEulerAngles = new Vector3((float) sampa.mpa.e, (float) sampa.mpa.azimuth, 0);

        moonLight.SetIntensity(Mathf.Lerp(0, MOON_MAX_INTENSITY, (float) MoonPhaseConsole.Moon.Calculate(sceneDateTime).Visibility/100));
    }
}
