using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class WeatherControl : MonoBehaviour
{
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private TMP_Dropdown cloudsValue;
    [SerializeField] private TMP_Dropdown precipitationInput;
    [SerializeField] private Slider precipitationSlider;
    [SerializeField] private TMP_Text precipitationValue;
    [SerializeField] private Slider fogSlider;
    [SerializeField] private TMP_Text fogValue;

    void Awake()
    {
        Assert.IsNotNull(weatherManager);
        Assert.IsNotNull(cloudsValue);
        Assert.IsNotNull(precipitationInput);
        Assert.IsNotNull(precipitationSlider);
        Assert.IsNotNull(precipitationValue);
        Assert.IsNotNull(fogSlider);
        Assert.IsNotNull(fogValue);

        cloudsValue.AddOptions(weatherManager.GetCloudsTypes());
    }

    void Start()
    {
        precipitationInput.onValueChanged.AddListener(change => ChangeWeather());
        precipitationSlider.onValueChanged.AddListener(change => ChangeWeather());
        fogSlider.onValueChanged.AddListener(change => ChangeWeather());
        cloudsValue.onValueChanged.AddListener(change => ChangeWeather());
    }

    private void ChangeWeather()
    {
        precipitationValue.text = precipitationSlider.value.ToString("0.00");
        fogValue.text = fogSlider.value.ToString("0.00");

        Weather weather = new Weather();
        weather.Snow = precipitationInput.value == 1;
        weather.Precipitation = precipitationSlider.value;
        weather.Fog = fogSlider.value;
        weather.Clouds = cloudsValue.options[cloudsValue.value].text;
        weatherManager.ChangeWeather(weather);
    }
}
