using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class EnvironmentLightControl : MonoBehaviour
{
    [SerializeField]
    private  Slider lightIntensity;
    [SerializeField]
    private  Slider lightTemperature;
    [SerializeField]
    private  Text lightIntensityText;
    [SerializeField]
    private  Text lightTemperatureText;
    [SerializeField]
    private EnvironmentLightManager environmentLightManager;

    void Awake()
    {
        Assert.IsNotNull(lightIntensity);
        Assert.IsNotNull(lightTemperature);
        Assert.IsNotNull(lightIntensityText);
        Assert.IsNotNull(lightTemperatureText);
        Assert.IsNotNull(environmentLightManager);
    }

    void Start()
    {
        lightIntensity.onValueChanged.AddListener(IntensityChanged);
        lightTemperature.onValueChanged.AddListener(temperatureChanged);

        lightIntensity.value = environmentLightManager.GetIntensity();
        lightTemperature.value = environmentLightManager.GetTemperature();
    }

    private void IntensityChanged(float value)
    {
        environmentLightManager.SetIntensity(value);
        lightIntensityText.text = lightIntensity.value + " Lux";
    }

    private void temperatureChanged(float value)
    {
        environmentLightManager.SetTemperature(value);
        lightTemperatureText.text = lightTemperature.value + " Kelvin";
    }
}
