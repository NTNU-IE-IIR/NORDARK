using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    private const int RAIN_MAX_PARTICULES = 10000;
    private const int RAIN_MAX_RATE_OVER_TIME = 2000;
    private const int SNOW_MAX_PARTICULES = 100000;
    private const int SNOW_MAX_RATE_OVER_TIME = 2000;
    [SerializeField] private Volume fogVolume;
    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private Transform cloudsParent;
    private Dictionary<string, Volume> clouds;
    
    void Awake()
    {
        Assert.IsNotNull(fogVolume);
        Assert.IsNotNull(rainParticles);
        Assert.IsNotNull(snowParticles);
        Assert.IsNotNull(cloudsParent);

        clouds = new Dictionary<string, Volume>();
        foreach (Transform cloudVolume in cloudsParent) {
            clouds.Add(cloudVolume.name, cloudVolume.GetComponent<Volume>());
        }
    }
    
    void Start()
    {
        ChangeWeather(new Weather());
    }

    public List<string> GetCloudsTypes()
    {
        List<string> cloudsTypes = new List<string>{ "Clear" };
        foreach (string cloudType in clouds.Keys) {
            cloudsTypes.Add(cloudType);
        }
        return cloudsTypes;
    }

    public void ChangeWeather(Weather weather)
    {
        fogVolume.weight = weather.Fog;

        foreach (Volume cloudType in clouds.Values) {
            cloudType.weight = 0;
        }
        if (clouds.ContainsKey(weather.Clouds)) {
            clouds[weather.Clouds].weight = 1;
        }

        Shader.SetGlobalFloat("_Snow_Opacity", 0);
        Shader.SetGlobalFloat("_Wetness", 0);
        if (weather.Snow) {
            Shader.SetGlobalFloat("_Snow_Opacity", weather.Precipitation);

            rainParticles.gameObject.SetActive(false);
            snowParticles.gameObject.SetActive(true);

            ParticleSystem.MainModule mainParticule = snowParticles.main;
            mainParticule.maxParticles = (int) Mathf.Lerp(0, SNOW_MAX_PARTICULES, weather.Precipitation);
            
            ParticleSystem.EmissionModule emissionParticule = snowParticles.emission;
            emissionParticule.rateOverTime = (int) Mathf.Lerp(0, SNOW_MAX_RATE_OVER_TIME, weather.Precipitation);
        } else {
            Shader.SetGlobalFloat("_Wetness", weather.Precipitation);

            rainParticles.gameObject.SetActive(true);
            snowParticles.gameObject.SetActive(false);

            ParticleSystem.MainModule mainParticule = rainParticles.main;
            mainParticule.maxParticles = (int) Mathf.Lerp(0, RAIN_MAX_PARTICULES, weather.Precipitation);
            
            ParticleSystem.EmissionModule emissionParticule = rainParticles.emission;
            emissionParticule.rateOverTime = (int) Mathf.Lerp(0, RAIN_MAX_RATE_OVER_TIME, weather.Precipitation);
        }
    }
}
