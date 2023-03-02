using UnityEngine;
using UnityEngine.Assertions;

public class LuminanceMapManager : MonoBehaviour
{
    public const float DEFAULT_MINIMUM_VALUE = MINIMUM_VALUE;
    public const float DEFAULT_MAXIMUM_VALUE = 10000;
    private const float MINIMUM_VALUE = 0.01f;
    [SerializeField] private GameObject LuminanceMapPassAndVolume;
    [SerializeField] private Material luminanceMapPassMaterial;
    [SerializeField] private LuminanceMapLegend luminanceMapLegend;
    private float minValue;
    private float maxValue;
    private bool isLog;
    
    void Awake()
    {
        Assert.IsNotNull(LuminanceMapPassAndVolume);
        Assert.IsNotNull(luminanceMapPassMaterial);
        Assert.IsNotNull(luminanceMapLegend);

        luminanceMapPassMaterial.SetColorArray("colors", luminanceMapLegend.GetColors());
        minValue = DEFAULT_MINIMUM_VALUE;
        maxValue = DEFAULT_MAXIMUM_VALUE;
        isLog = true;
    }

    void Start()
    {
        SetScale();
    }

    public void SetMinValue(float minValue)
    {
        this.minValue = Mathf.Max(minValue, MINIMUM_VALUE);
        SetScale();
    }

    public void SetMaxValue(float maxValue)
    {
        this.maxValue = Mathf.Max(maxValue, MINIMUM_VALUE);
        SetScale();
    }

    public void SetScaleType(bool isLog)
    {
        this.isLog = isLog;
        SetScale();
    }

    public void DisplayLuminanceMap(bool display)
    {
        LuminanceMapPassAndVolume.SetActive(display);
        luminanceMapLegend.SetActive(display);
    }

    private void SetScale()
    {
        int numberOfColors = luminanceMapLegend.GetNumberOfColors();
        float[] values = new float[numberOfColors];

        float minValueLog = Mathf.Log10(minValue);
        float maxValueLog = Mathf.Log10(maxValue);

        for (int i=0; i<numberOfColors; ++i) {
            if (isLog) {
                values[i] = Mathf.Pow(10, minValueLog + i * (maxValueLog - minValueLog) / (numberOfColors-1));
            } else {
                values[i] = minValue + i * (maxValue - minValue) / (numberOfColors-1);
            }
        }

        luminanceMapLegend.SetValues(values);

        luminanceMapPassMaterial.SetInt("numberOfColors", values.Length);
        luminanceMapPassMaterial.SetFloatArray("values", values);
    }
}