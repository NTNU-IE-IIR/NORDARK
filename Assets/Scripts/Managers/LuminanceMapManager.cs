using UnityEngine;
using UnityEngine.Assertions;

public class LuminanceMapManager : MonoBehaviour
{
    public const int LUMINANCE_IN_LUMINANCE_MAP_LAYER = 13;
    public const float DEFAULT_MINIMUM_VALUE = MINIMUM_VALUE;
    public const float DEFAULT_MAXIMUM_VALUE = 10000;
    private const float MINIMUM_VALUE = 0.01f;
    private const int NUMBER_OF_PIXELS_AROUND_CURSOR = 10;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private LuminanceMapControl luminanceMapControl;
    [SerializeField] private GameObject luminanceMapPassAndVolume;
    [SerializeField] private UnityEngine.Rendering.HighDefinition.CustomPassVolume luminancePassAndVolume;
    [SerializeField] private Material luminanceMapPassMaterial;
    [SerializeField] private LuminanceMapLegend luminanceMapLegend;
    private float minValue;
    private float maxValue;
    private bool isLog;
    private bool isLuminanceMapActive;
    
    void Awake()
    {
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(luminanceMapControl);
        Assert.IsNotNull(luminanceMapPassAndVolume);
        Assert.IsNotNull(luminancePassAndVolume);
        Assert.IsNotNull(luminanceMapPassMaterial);
        Assert.IsNotNull(luminanceMapLegend);

        luminanceMapPassMaterial.SetColorArray("colors", luminanceMapLegend.GetColors());
        minValue = DEFAULT_MINIMUM_VALUE;
        maxValue = DEFAULT_MAXIMUM_VALUE;
        isLog = true;
        isLuminanceMapActive = false;
    }

    void Start()
    {
        SetScale();
    }

    void Update()
    {
        if (isLuminanceMapActive) {
            Color[] colors = sceneCamerasManager.GetPixelColorsOfLuminanceMapCameraPointedAroundCursor(NUMBER_OF_PIXELS_AROUND_CURSOR);

            if (colors == null || colors.Length == 0) {
                luminanceMapControl.SetPointedValue(-1);
            } else {
                float luminance = 0;
                for (int i=0; i<colors.Length; ++i) {
                    luminance += 
                        Mathf.Round(colors[i].r * 100) * 100 +
                        Mathf.Round(colors[i].g * 100) * 1 +
                        Mathf.Round(colors[i].b * 100) * 0.01f
                    ;
                }
                luminance /= colors.Length;
                luminanceMapControl.SetPointedValue(luminance);
            }
        }
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
        luminanceMapPassAndVolume.SetActive(display);
        luminanceMapLegend.SetActive(display);
        isLuminanceMapActive = display;
        sceneCamerasManager.CreateOrDeleteLuminanceMapCamera(display);
        luminanceMapControl.SetPointedValue(-1);
    }

    public void AddCameraToLuminancePass(Camera camera)
    {
        luminancePassAndVolume.targetCamera = camera;
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