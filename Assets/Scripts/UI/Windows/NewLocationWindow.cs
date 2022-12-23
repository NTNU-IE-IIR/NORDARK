using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class NewLocationWindow : MonoBehaviour
{
    private const float MAX_LATITUDE_DIFFERENCE = 0.2f;
    private const float MAX_LONGITUDE_DIFFERENCE = 0.2f;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private Button close;
    [SerializeField] private TMP_InputField locationName;
    [SerializeField] private TMP_InputField locationLatitude;
    [SerializeField] private TMP_InputField locationLongitude;
    [SerializeField] private TMP_InputField locationAltitude;
    [SerializeField] private Toggle useLocationCoordinates;
    [SerializeField] private TMP_InputField cameraLatitude;
    [SerializeField] private TMP_InputField cameraLongitude;
    [SerializeField] private TMP_InputField cameraAltitude;
    [SerializeField] private TMP_InputField cameraAngleX;
    [SerializeField] private TMP_InputField cameraAngleY;
    [SerializeField] private TMP_InputField cameraAngleZ;
    [SerializeField] private Button create;
    [SerializeField] private TMP_Text error;

    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(close);
        Assert.IsNotNull(locationName);
        Assert.IsNotNull(locationLatitude);
        Assert.IsNotNull(locationLongitude);
        Assert.IsNotNull(locationAltitude);
        Assert.IsNotNull(useLocationCoordinates);
        Assert.IsNotNull(cameraLatitude);
        Assert.IsNotNull(cameraLongitude);
        Assert.IsNotNull(cameraAltitude);
        Assert.IsNotNull(cameraAngleX);
        Assert.IsNotNull(cameraAngleY);
        Assert.IsNotNull(cameraAngleZ);
        Assert.IsNotNull(create);
        Assert.IsNotNull(error);
    }

    void Start()
    {
        close.onClick.AddListener(Close);

        locationLatitude.onValueChanged.AddListener(value => UpdateCameraParameters(useLocationCoordinates.isOn));
        locationLongitude.onValueChanged.AddListener(value => UpdateCameraParameters(useLocationCoordinates.isOn));
        locationAltitude.onValueChanged.AddListener(value => UpdateCameraParameters(useLocationCoordinates.isOn));

        useLocationCoordinates.onValueChanged.AddListener(UpdateCameraParameters);
        
        create.onClick.AddListener(CreateLocation);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    private void Close()
    {
        error.text = "";
        gameObject.SetActive(false);
    }

    private void UpdateCameraParameters(bool isOn)
    {
        if (isOn) {
            cameraLatitude.text = locationLatitude.text;
            cameraLongitude.text = locationLongitude.text;
            cameraAltitude.text = locationAltitude.text;
        }
    }

    private void CreateLocation()
    {
        string errorMessage = IsInputValid();

        if (errorMessage == "") {
            Feature feature = new Feature();
            feature.Properties.Add("type", "location");
            feature.Properties.Add("name", locationName.text);
            feature.Properties.Add("cameraCoordinates", new List<double> {
                double.Parse(cameraLatitude.text),
                double.Parse(cameraLongitude.text),
                double.Parse(cameraAltitude.text)
            });
            feature.Properties.Add("cameraAngles", new List<float> {
                float.Parse(cameraAngleX.text),
                float.Parse(cameraAngleY.text),
                float.Parse(cameraAngleZ.text)
            });
            feature.Coordinates = new List<Vector3d> {new Vector3d(
                double.Parse(locationLatitude.text),
                double.Parse(locationLongitude.text),
                double.Parse(locationAltitude.text)
            )};
            locationsManager.Create(feature);

            Close();
        } else {
            error.text = errorMessage;
        }
    }

    private string IsInputValid()
    {
        if (locationName.text == "") {
            return "Empty name";
        } else if (Mathf.Abs(float.Parse(locationLatitude.text) - float.Parse(cameraLatitude.text)) > MAX_LATITUDE_DIFFERENCE) {
            return "Camera latitude too far away from location latitude";
        } else if (Mathf.Abs(float.Parse(locationLongitude.text) - float.Parse(cameraLongitude.text)) > MAX_LONGITUDE_DIFFERENCE) {
            return "Camera longitude too far away from location longitude";
        } else {
            return "";
        }
    }
}
