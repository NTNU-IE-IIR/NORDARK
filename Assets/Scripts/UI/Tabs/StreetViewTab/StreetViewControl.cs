using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class StreetViewControl : MonoBehaviour
{
  [SerializeField]
  private StreetViewManager streetViewManager;
  [SerializeField]
  private SunManager sunManager;
  [SerializeField]
  private LightsManager lightsManager;
  [SerializeField]
  private Slider sunLightIntensitySlider;
  [SerializeField]
  private Slider streetLightIntensitySlider;
  [SerializeField]
  private Slider streetLightHueSlider;
  [SerializeField]
  private Button streetLightHueResetButton;
  [SerializeField]
  private Slider heightSlider;
  [SerializeField]
  private TMP_Text heightLabel;
  [SerializeField]
  private Toggle superPowerCheckbox;
  [SerializeField]
  private TMP_Text characterTitleLabel;

  void Awake()
  {
    Assert.IsNotNull(sunManager);
    Assert.IsNotNull(lightsManager);
    Assert.IsNotNull(sunLightIntensitySlider);
    Assert.IsNotNull(streetLightIntensitySlider);
    Assert.IsNotNull(streetLightHueSlider);
    Assert.IsNotNull(streetLightHueResetButton);
    Assert.IsNotNull(heightSlider);
    Assert.IsNotNull(heightLabel);
    Assert.IsNotNull(superPowerCheckbox);
    Assert.IsNotNull(characterTitleLabel);
  }

  void Start()
  {
    sunLightIntensitySlider.onValueChanged.AddListener(SunIntensityChanged);
    streetLightIntensitySlider.onValueChanged.AddListener(lightsManager.changeAllLightIntensity);
    streetLightHueSlider.onValueChanged.AddListener(lightsManager.changeAllLightHue);
    streetLightHueResetButton.onClick.AddListener(delegate
    {
      streetLightHueSlider.value = 0;
      lightsManager.resetAllLightsHue();
    });
    heightSlider.onValueChanged.AddListener(delegate
    {
      streetViewManager.ChangeHeight(heightSlider.value);
      heightLabel.text = heightSlider.value + " cm";
      ChangeCharacterTitle();
    });
    superPowerCheckbox.onValueChanged.AddListener(delegate
    {
      streetViewManager.toggleSuperPower(superPowerCheckbox.isOn);
      ChangeCharacterTitle();
    });

    sunLightIntensitySlider.value = sunManager.GetIntensity();
    heightLabel.text = heightSlider.value + " cm";
    ChangeCharacterTitle();
  }

  private void ChangeCharacterTitle()
  {
    float height = heightSlider.value;
    bool superPowers = superPowerCheckbox.isOn;

    string prefix = "";
    string postfix = superPowers ? " with super powers" : "";

    if (height < 170)
    {
      prefix = "Child";
    }
    else
    {
      prefix = "Adult";
    }

    characterTitleLabel.text = prefix + postfix;
  }

  private void SunIntensityChanged(float value)
  {
    sunManager.SetIntensity(value);
  }
}
