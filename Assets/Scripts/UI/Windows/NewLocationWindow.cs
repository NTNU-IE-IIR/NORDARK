using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class NewLocationWindow : MonoBehaviour
{
    private const float DEFAULT_SIZE = 1;
    private const float MAX_LATITUDE_DIFFERENCE = 0.2f;
    private const float MAX_LONGITUDE_DIFFERENCE = 0.2f;
    private const float DEFAULT_CAMERA_HEIGHT = 10;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private RectTransform content;
    [SerializeField] private Button close;
    [SerializeField] private TMP_InputField locationName;
    [SerializeField] private TMP_Dropdown locationType;
    [SerializeField] private RectTransform worldLocation;
    [SerializeField] private TMP_InputField locationLatitude;
    [SerializeField] private TMP_InputField locationLongitude;
    [SerializeField] private TMP_InputField locationAltitude;
    [SerializeField] private TMP_InputField locationSize;
    [SerializeField] private Toggle useLocationCoordinates;
    [SerializeField] private TMP_InputField cameraLatitude;
    [SerializeField] private TMP_InputField cameraLongitude;
    [SerializeField] private TMP_InputField cameraAltitude;
    [SerializeField] private TMP_InputField cameraAngleX;
    [SerializeField] private TMP_InputField cameraAngleY;
    [SerializeField] private TMP_InputField cameraAngleZ;
    [SerializeField] private Button create;
    [SerializeField] private TMP_Text error;
    private Vector2 contentInitialSize;

    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(content);
        Assert.IsNotNull(close);
        Assert.IsNotNull(locationName);
        Assert.IsNotNull(locationType);
        Assert.IsNotNull(worldLocation);
        Assert.IsNotNull(locationLatitude);
        Assert.IsNotNull(locationLongitude);
        Assert.IsNotNull(locationAltitude);
        Assert.IsNotNull(locationSize);
        Assert.IsNotNull(useLocationCoordinates);
        Assert.IsNotNull(cameraLatitude);
        Assert.IsNotNull(cameraLongitude);
        Assert.IsNotNull(cameraAltitude);
        Assert.IsNotNull(cameraAngleX);
        Assert.IsNotNull(cameraAngleY);
        Assert.IsNotNull(cameraAngleZ);
        Assert.IsNotNull(create);
        Assert.IsNotNull(error);

        contentInitialSize = content.sizeDelta;
    }

    void Start()
    {
        close.onClick.AddListener(Close);

        locationType.onValueChanged.AddListener(value => {
            if (value == 0) {
                worldLocation.gameObject.SetActive(true);
                content.sizeDelta = contentInitialSize;
            } else {
                content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y - worldLocation.sizeDelta.y);
                worldLocation.gameObject.SetActive(false);
            }
        });

        locationLatitude.onValueChanged.AddListener(value => UpdateCameraParameters(useLocationCoordinates.isOn));
        locationLongitude.onValueChanged.AddListener(value => UpdateCameraParameters(useLocationCoordinates.isOn));
        locationAltitude.onValueChanged.AddListener(value => UpdateCameraParameters(useLocationCoordinates.isOn));

        useLocationCoordinates.onValueChanged.AddListener(UpdateCameraParameters);
        
        create.onClick.AddListener(CreateLocation);
    }

    public void Open()
    {
        locationName.text = "";
        locationType.value = 0;
        locationLatitude.text = "0";
        locationLongitude.text = "0";
        locationAltitude.text = "0";
        locationSize.text = DEFAULT_SIZE.ToString();
        useLocationCoordinates.isOn = true;
        UpdateCameraParameters(true);
        cameraAngleX.text = "0";
        cameraAngleY.text = "0";
        cameraAngleZ.text = "0";
        error.text = "";
        gameObject.SetActive(true);
    }

    private void Close()
    {
        error.text = "";
        gameObject.SetActive(false);
    }

    private void UpdateCameraParameters(bool isOn)
    {
        cameraLatitude.interactable = !isOn;
        cameraLongitude.interactable = !isOn;
        cameraAltitude.interactable = !isOn;

        if (isOn) {
            cameraLatitude.text = locationLatitude.text;
            cameraLongitude.text = locationLongitude.text;
            cameraAltitude.text = (float.Parse(locationAltitude.text) + DEFAULT_CAMERA_HEIGHT).ToString();
        }
    }

    private void CreateLocation()
    {
        string errorMessage = IsInputValid();

        if (errorMessage == "") {
            Location location = new Location();
            location.Name = locationName.text;
            location.Type = locationType.value == 0 ? Location.TerrainType.Map : Location.TerrainType.Basic;

            if (location.Type == Location.TerrainType.Map) {
                location.Coordinate = new Coordinate(
                    double.Parse(locationLatitude.text),
                    double.Parse(locationLongitude.text),
                    double.Parse(locationAltitude.text)
                );
                location.CameraCoordinates = new Coordinate(
                    double.Parse(cameraLatitude.text),
                    double.Parse(cameraLongitude.text),
                    double.Parse(cameraAltitude.text)
                );
                location.CameraAngles = new Vector3(
                    float.Parse(cameraAngleX.text),
                    float.Parse(cameraAngleY.text),
                    float.Parse(cameraAngleZ.text)
                );
                location.Zoom = mapManager.GetZoomCoveringSizeAtLatitude(
                    1000 * float.Parse(locationSize.text),
                    (float) location.Coordinate.latitude
                );
            } else {
                location.CameraCoordinates = new Coordinate(0, 0, BasicLocationManager.DEFAULT_CAMERA_HEIGHT);
                location.CameraAngles = new Vector3(BasicLocationManager.DEFAULT_CAMERA_ANGLE_X, 0, 0);
            }

            locationsManager.AddLocation(location);
            Close();
        } else {
            error.text = errorMessage;
        }
    }

    private string IsInputValid()
    {
        if (locationName.text == "") {
            return "Empty name";
        } else if (locationsManager.DoesLocationNameAlreadyExists(locationName.text)) {
            return "A location with the same name already exists";
        } else if (Mathf.Abs(float.Parse(locationLatitude.text) - float.Parse(cameraLatitude.text)) > MAX_LATITUDE_DIFFERENCE) {
            return "Camera latitude too far away from location latitude";
        } else if (Mathf.Abs(float.Parse(locationLongitude.text) - float.Parse(cameraLongitude.text)) > MAX_LONGITUDE_DIFFERENCE) {
            return "Camera longitude too far away from location longitude";
        } else {
            return "";
        }
    }
}
