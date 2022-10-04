using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using System.Collections.Generic;

public class StreetViewControl : MonoBehaviour
{
  [SerializeField]
  private TMP_Dropdown preset;
  [SerializeField]
  private StreetViewManager streetViewManager;

  void Awake()
  {
    Assert.IsNotNull(preset);

    preset.AddOptions(new List<string> { "Child", "Adult", "UFO" });
  }

  void Start()
  {
    preset.onValueChanged.AddListener(delegate
    {
      streetViewManager.ChangePreset(preset.options[preset.value].text);
    });
  }
}
