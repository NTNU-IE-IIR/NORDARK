using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class StreetViewControl : MonoBehaviour
{
  [SerializeField] private StreetViewManager streetViewManager;
  [SerializeField] private Slider heightSlider;
  [SerializeField] private TMP_Text heightLabel;
  [SerializeField] private Toggle superPowerCheckbox;
  [SerializeField] private TMP_Text characterTitleLabel;

  void Awake()
  {
    Assert.IsNotNull(streetViewManager);
    Assert.IsNotNull(heightSlider);
    Assert.IsNotNull(heightLabel);
    Assert.IsNotNull(superPowerCheckbox);
    Assert.IsNotNull(characterTitleLabel);
  }

  void Start()
  {
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

    heightLabel.text = heightSlider.value + " cm";
    ChangeCharacterTitle();
  }

  private void ChangeCharacterTitle()
  {
    float height = heightSlider.value;
    bool superPowers = superPowerCheckbox.isOn;

    string prefix = "";

    if(height < 30){
      prefix = "Insect";
    }
    else if (height < 170)
    {
      prefix = "Child";
    }
    else
    {
      prefix = "Adult";
    }

    string postfix = superPowers ? " with super powers" : "";
    if(prefix == "Insect" && superPowers){
      postfix = " with wings";
    }

    characterTitleLabel.text = prefix + postfix;
  }
}