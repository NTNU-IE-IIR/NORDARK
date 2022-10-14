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
    private TMP_Text lightIntensityText;
    [SerializeField]
    private SunManager sunManager;

    void Awake()
    {
        Assert.IsNotNull(lightIntensity);
        Assert.IsNotNull(lightIntensityText);
        Assert.IsNotNull(sunManager);
    }

    void Start()
    {
        lightIntensity.onValueChanged.AddListener(IntensityChanged);

        lightIntensity.value = sunManager.GetIntensity();
    }

    private void IntensityChanged(float value)
    {
        sunManager.SetIntensity(value);
        lightIntensityText.text = lightIntensity.value + " Lux";
    }
}