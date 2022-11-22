using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class WeatherControl : MonoBehaviour
{
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private TMP_Dropdown precipitationInput;
    [SerializeField] private Slider precipitationValue;
    [SerializeField] private Slider fogValue;
    [SerializeField] private TMP_Dropdown cloudsValue;

    void Awake()
    {
        Assert.IsNotNull(weatherManager);
        Assert.IsNotNull(precipitationInput);
        Assert.IsNotNull(precipitationValue);
        Assert.IsNotNull(fogValue);
        Assert.IsNotNull(cloudsValue);

        cloudsValue.AddOptions(weatherManager.GetCloudsTypes());
    }

    void Start()
    {
        precipitationInput.onValueChanged.AddListener(change => ChangeWeather());
        precipitationValue.onValueChanged.AddListener(change => ChangeWeather());
        fogValue.onValueChanged.AddListener(change => ChangeWeather());
        cloudsValue.onValueChanged.AddListener(change => ChangeWeather());
    }

    private void ChangeWeather()
    {
        Weather weather = new Weather();
        weather.Snow = precipitationInput.value == 1;
        weather.Precipitation = precipitationValue.value;
        weather.Fog = fogValue.value;
        weather.Clouds = cloudsValue.options[cloudsValue.value].text;
        weatherManager.ChangeWeather(weather);
    }
}
