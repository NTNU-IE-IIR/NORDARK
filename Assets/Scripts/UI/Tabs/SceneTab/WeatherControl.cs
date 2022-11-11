using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class WeatherControl : MonoBehaviour
{
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private TMP_Dropdown weatherInput;
    [SerializeField] private Toggle toggleSnow;

    void Awake()
    {
        Assert.IsNotNull(weatherManager);
        Assert.IsNotNull(weatherInput);
        Assert.IsNotNull(toggleSnow);
    }

    void Start()
    {
        weatherInput.onValueChanged.AddListener(change => ChangeWeather());
        toggleSnow.onValueChanged.AddListener(weatherManager.SetSnow);
    }

    private void ChangeWeather()
    {
        weatherManager.ChangeWeather((WeatherManager.Weather) weatherInput.value);
    }
}
