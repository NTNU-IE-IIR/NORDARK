using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class SunControl : MonoBehaviour
{
    [SerializeField]
    private Slider lightIntensity;
    [SerializeField]
    private Slider lightTemperature;
    [SerializeField]
    private TMP_Text lightIntensityText;
    [SerializeField]
    private TMP_Text lightTemperatureText;
    [SerializeField]
    private SunManager sunManager;

    void Awake()
    {
        Assert.IsNotNull(lightIntensity);
        Assert.IsNotNull(lightTemperature);
        Assert.IsNotNull(lightIntensityText);
        Assert.IsNotNull(lightTemperatureText);
        Assert.IsNotNull(sunManager);
    }

    void Start()
    {
        lightIntensity.onValueChanged.AddListener(IntensityChanged);
        lightTemperature.onValueChanged.AddListener(temperatureChanged);

        lightIntensity.value = sunManager.GetIntensity();
        lightTemperature.value = sunManager.GetTemperature();
    }

    private void IntensityChanged(float value)
    {
        sunManager.SetIntensity(value);
        lightIntensityText.text = lightIntensity.value + " Lux";
    }

    private void temperatureChanged(float value)
    {
        sunManager.SetTemperature(value);
        lightTemperatureText.text = lightTemperature.value + " Kelvin";
    }
}