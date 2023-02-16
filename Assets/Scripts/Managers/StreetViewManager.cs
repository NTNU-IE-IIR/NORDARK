using UnityEngine;
using UnityEngine.Assertions;

public class StreetViewManager : MonoBehaviour
{
    private const float MIN_HEIGHT = 2f;
    private const float MAX_HEIGHT = 188f;
    private const float MIN_SCALE = 0.05f;
    private const float MAX_SCALE = 1.75f;
    private const float SCALE_TO_METER = 1.8f;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject FPSCharacter;
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private float sensitivity = 5.0f;
    private Rigidbody rb;
    private GameObject FPSCamera;
    private bool isStreetViewActive;
    private float yaw;
    private float pitch;
    private bool canFly;
    private bool cursorLock;

    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(mainCamera);
        Assert.IsNotNull(FPSCharacter);

        isStreetViewActive = false;
        yaw = 0.0f;
        pitch = 0.0f;
        canFly = false;
        cursorLock = false;
    }

    void Start()
    {
        rb = FPSCharacter.GetComponent<Rigidbody>();
        FPSCamera = FPSCharacter.transform.Find("FPS Camera").gameObject;
    }

    void Update()
    {
        if (isStreetViewActive && Input.GetKeyDown(KeyCode.Escape)) {
            ToggleCursorLock();
        }

        if (!cursorLock) return;

        if (canFly) {
            if (Input.GetKey(KeyCode.Space)) {
                rb.velocity = new Vector3(rb.velocity.x, 5, rb.velocity.y);
            } else if (Input.GetKey(KeyCode.LeftControl)) {
                rb.velocity = new Vector3(rb.velocity.x, -5, rb.velocity.y);
            } else {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.y);
            }
        }
        Look();
    }

    void FixedUpdate()
    {
        if (cursorLock) {
            Movement();
        }
    }

    public void DisplayCameraPreview(bool shouldDisplay)
    {
        if (shouldDisplay) {
            isStreetViewActive = true;

            if (!cursorLock) {
                ToggleCursorLock();
            }

            mainCamera.SetActive(false);
            FPSCharacter.SetActive(true);

            Location currentLocation = locationsManager.GetCurrentLocation();
            if (currentLocation != null) {
                rb.MovePosition(mapManager.GetUnityPositionFromCoordinates(currentLocation.CameraCoordinates, true) + new Vector3(0, 5, 0));
            }

            // Starting with the child height (124cm)
            this.ChangeHeight(124);
        } else {
            isStreetViewActive = false;

            if (cursorLock) {
                ToggleCursorLock();
            }

            mainCamera.SetActive(true);
            FPSCharacter.SetActive(false);
        }
    }

    public void ChangeHeight(float value)
    {
        float multiplier = (value - MIN_HEIGHT) / (MAX_HEIGHT - MIN_HEIGHT);
        float scale = MIN_SCALE + multiplier * (MAX_SCALE - MIN_SCALE) / SCALE_TO_METER;

        rb.transform.localScale = new Vector3(
            rb.transform.localScale.x,
            scale,
            rb.transform.localScale.z
        );
    }

    public void ToggleSuperPower(bool isActive)
    {
        if (isActive) {
            canFly = true;
            rb.useGravity = false;
        } else {
            rb.useGravity = true;
            canFly = false;
        }
    }

    void ToggleCursorLock()
    {
        if (!cursorLock) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLock = true;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorLock = false;
        }
    }

    void Look()
    {
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        FPSCamera.transform.localRotation = Quaternion.Euler(pitch, yaw, 0);
    }

    void Movement()
    {
        Vector2 axis = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")) * walkSpeed;
        Vector3 forward = new Vector3(-FPSCamera.transform.right.z, 0.0f, FPSCamera.transform.right.x);
        Vector3 wishDirection = (forward * axis.x + FPSCamera.transform.right * axis.y + Vector3.up * rb.velocity.y);
        rb.velocity = wishDirection;
    }
}