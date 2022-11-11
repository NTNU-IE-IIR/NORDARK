using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    public enum Weather
    {
        ClearSky,
        PartlyCloudy,
        Cloudy,
        Foggy,
        LightRain,
        HeavyRain,
        Snow
    }

    [SerializeField] Volume fogVolume;
    [SerializeField] Volume sparseCloudVolume;
    [SerializeField] Volume cloudyCloudVolume;
    [SerializeField] Volume overcastCloudVolume;
    [SerializeField] Volume stormyCloudVolume;
    [SerializeField] GameObject lightRainParticles;
    [SerializeField] GameObject heavyRainParticles;
    [SerializeField] GameObject snowParticles;

    void Awake()
    {
        Assert.IsNotNull(fogVolume);
        Assert.IsNotNull(sparseCloudVolume);
        Assert.IsNotNull(cloudyCloudVolume);
        Assert.IsNotNull(overcastCloudVolume);
        Assert.IsNotNull(stormyCloudVolume);
        Assert.IsNotNull(lightRainParticles);
        Assert.IsNotNull(heavyRainParticles);
        Assert.IsNotNull(snowParticles);
    }
    
    void Start()
    {
        ChangeWeather(Weather.ClearSky);
    }

    public void ChangeWeather(Weather weather)
    {
        fogVolume.weight = 0;
        sparseCloudVolume.weight = 0;
        cloudyCloudVolume.weight = 0;
        overcastCloudVolume.weight = 0;
        stormyCloudVolume.weight = 0;
        lightRainParticles.SetActive(false);
        heavyRainParticles.SetActive(false);
        snowParticles.SetActive(false);

        switch (weather) {
            case Weather.PartlyCloudy:
                sparseCloudVolume.weight = 1;
                break;
            case Weather.Cloudy:
                cloudyCloudVolume.weight = 1;
                break;
            case Weather.Foggy:
                fogVolume.weight = 1;
                overcastCloudVolume.weight = 1;
                break;
            case Weather.LightRain:
                stormyCloudVolume.weight = 1;
                lightRainParticles.SetActive(true);
                break;
            case Weather.HeavyRain:
                overcastCloudVolume.weight = 1;
                heavyRainParticles.SetActive(true);
                break;
            case Weather.Snow:
                overcastCloudVolume.weight = 1;
                snowParticles.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void SetSnow(bool snow)
    {
        Shader.SetGlobalFloat("_Snow_Opacity", snow? 1 : 0);
    }
}
