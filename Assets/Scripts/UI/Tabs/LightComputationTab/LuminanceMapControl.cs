using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class LuminanceMapControl : MonoBehaviour
{
    [SerializeField] private LuminanceMapManager luminanceMapManager;
    [SerializeField] private TMP_InputField minValue;
    [SerializeField] private TMP_InputField maxValue;
    [SerializeField] private Toggle logScale;
    [SerializeField] private TMP_Text pointedValue;
    [SerializeField] private Toggle displayLuminanceMap;

    void Awake()
    {
        Assert.IsNotNull(luminanceMapManager);
        Assert.IsNotNull(minValue);
        Assert.IsNotNull(maxValue);
        Assert.IsNotNull(logScale);
        Assert.IsNotNull(pointedValue);
        Assert.IsNotNull(displayLuminanceMap);
    }

    void Start()
    {
        minValue.text = LuminanceMapManager.DEFAULT_MINIMUM_VALUE.ToString();
        maxValue.text = LuminanceMapManager.DEFAULT_MAXIMUM_VALUE.ToString();

        minValue.onEndEdit.AddListener(value => luminanceMapManager.SetMinValue(float.Parse(value)));
        maxValue.onEndEdit.AddListener(value => luminanceMapManager.SetMaxValue(float.Parse(value)));
        logScale.onValueChanged.AddListener(luminanceMapManager.SetScaleType);
        displayLuminanceMap.onValueChanged.AddListener(luminanceMapManager.DisplayLuminanceMap);
    }

    public void SetPointedValue(float value)
    {
        if (value < 0) {
            pointedValue.text = "";
        } else {
            pointedValue.text = "Value pointer by cursor:\n" + value.ToString("0.00") + " cd/m²";
        }
    }
}
