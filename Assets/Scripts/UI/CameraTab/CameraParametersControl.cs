using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class CameraParametersControl : MonoBehaviour
{
    [SerializeField]
    private CamerasManager camerasManager;
    [SerializeField]
    private InputField sensorX;
    [SerializeField]
    private InputField sensorY;
    [SerializeField]
    private InputField ISO;
    [SerializeField]
    private InputField shutterSpeed;
    [SerializeField]
    private InputField focalLength;
    [SerializeField]
    private Slider apertureSlider;
    [SerializeField]
    private InputField apertureInput;
    [SerializeField]
    private InputField shiftX;
    [SerializeField]
    private InputField shiftY;

    void Awake()
    {
        Assert.IsNotNull(camerasManager);
        Assert.IsNotNull(sensorX);
        Assert.IsNotNull(sensorY);
        Assert.IsNotNull(ISO);
        Assert.IsNotNull(shutterSpeed);
        Assert.IsNotNull(focalLength);
        Assert.IsNotNull(apertureSlider);
        Assert.IsNotNull(apertureInput);
        Assert.IsNotNull(shiftX);
        Assert.IsNotNull(shiftY);
    }

    void Start()
    {
        sensorX.onEndEdit.AddListener(change => SensorChanged());
        sensorY.onEndEdit.AddListener(change => SensorChanged());
        ISO.onEndEdit.AddListener(change => ISOChanged());
        shutterSpeed.onEndEdit.AddListener(change => ShutterSpeedChanged());
        focalLength.onEndEdit.AddListener(change => FocalLengthChanged());
        apertureSlider.onValueChanged.AddListener(change => ApertureSliderChanged());
        apertureInput.onEndEdit.AddListener(change => ApertureInputChanged());
        shiftX.onEndEdit.AddListener(change => ShiftChanged());
        shiftY.onEndEdit.AddListener(change => ShiftChanged());
    }

    public void UpdateParameters(CameraParameters cameraParameters)
    {
        sensorX.text = cameraParameters.SensorSize.x.ToString();
        sensorY.text = cameraParameters.SensorSize.y.ToString();
        ISO.text = cameraParameters.ISO.ToString();
        shutterSpeed.text = cameraParameters.ShutterSpeed.ToString();
        focalLength.text = cameraParameters.FocalLength.ToString();
        apertureSlider.value = cameraParameters.Aperture;
        apertureInput.text = cameraParameters.Aperture.ToString();
        shiftX.text = cameraParameters.Shift.x.ToString();
        shiftY.text = cameraParameters.Shift.y.ToString();
    }

    public CameraParameters GetCameraParameters()
    {
        CameraParameters cameraParameters = new CameraParameters();
        cameraParameters.SensorSize = new Vector2(InputToFloat(sensorX), InputToFloat(sensorY));
        cameraParameters.ShutterSpeed = InputToFloat(shutterSpeed);
        cameraParameters.FocalLength = InputToFloat(focalLength);
        cameraParameters.Aperture = apertureSlider.value;
        cameraParameters.Shift = new Vector2(InputToFloat(shiftX), InputToFloat(shiftY));
        return cameraParameters;
    }

    private void SensorChanged()
    {
        camerasManager.SetSensorSize(new Vector2(InputToFloat(sensorX), InputToFloat(sensorY)));
    }

    private void ISOChanged()
    {
        camerasManager.SetISO((int) InputToFloat(ISO));
    }

    private void ShutterSpeedChanged()
    {
        camerasManager.SetShutterSpeed(InputToFloat(shutterSpeed));
    }

    private void FocalLengthChanged()
    {
        camerasManager.SetFocalLength(InputToFloat(focalLength));
    }

    private void ApertureSliderChanged()
    {
        apertureInput.text = apertureSlider.value.ToString();
        camerasManager.SetAperture(apertureSlider.value);
    }

    private void ApertureInputChanged()
    {
        float apertureValue = InputToFloat(apertureInput);
        apertureValue = System.Math.Max(apertureValue, apertureSlider.minValue);
        apertureSlider.value = apertureValue;
        camerasManager.SetAperture(apertureValue);
    }

    private void ShiftChanged()
    {
        camerasManager.SetShift(new Vector2(InputToFloat(shiftX), InputToFloat(shiftY)));
    }

    private float InputToFloat(InputField input)
    {
        float value = 0;
        if (input.text != "") {
            value = float.Parse(input.text);
        }
        return value;
    }
}
