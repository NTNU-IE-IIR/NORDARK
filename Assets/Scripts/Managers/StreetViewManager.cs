using UnityEngine;
using UnityEngine.Assertions;


public class StreetViewManager : MonoBehaviour
{
  [SerializeField] private LocationsManager locationsManager;
  [SerializeField] private MapManager mapManager;
  [SerializeField] private GameObject mainCamera;
  [SerializeField] private GameObject FPSCharacter;
  private GameObject FPSCamera;

  private bool isStreetViewActive = false;

  public float walkSpeed = 4.0f;
  public float sensitivity = 5.0f;
  private float yaw = 0.0f;
  private float pitch = 0.0f;
  private Rigidbody rb;
  private bool canFly = false;

  private bool cursorLock = false;

  const float MIN_HEIGHT = 2f;
  const float MAX_HEIGHT = 188f;
  const float MIN_SCALE = 0.05f;
  const float MAX_SCALE = 1.75f;

  public void DisplayCameraPreview(bool shouldDisplay)
  {
    if (shouldDisplay)
    {
      isStreetViewActive = true;

      if (!cursorLock)
      {
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
    }
    else
    {
      isStreetViewActive = false;

      if (cursorLock)
      {
        ToggleCursorLock();
      }

      mainCamera.SetActive(true);
      FPSCharacter.SetActive(false);
    }
  }

  public void ChangeHeight(float value)
  {
    float multiplier = (value - MIN_HEIGHT) / (MAX_HEIGHT - MIN_HEIGHT);
    float scale = MIN_SCALE + multiplier * (MAX_SCALE - MIN_SCALE);

    rb.transform.localScale = new Vector3(
      rb.transform.localScale.x,
      scale,
      rb.transform.localScale.z
    );
  }

  public void toggleSuperPower(bool isActive)
  {
    if (isActive)
    {
      canFly = true;
      rb.useGravity = false;
    }
    else
    {
      rb.useGravity = true;
      canFly = false;
    }
  }

  void Awake()
  {
    Assert.IsNotNull(locationsManager);
    Assert.IsNotNull(mapManager);
    Assert.IsNotNull(mainCamera);
    Assert.IsNotNull(FPSCharacter);
  }

  void Start()
  {
    rb = FPSCharacter.GetComponent<Rigidbody>();
    FPSCamera = FPSCharacter.transform.Find("FPS Camera").gameObject;
  }

  void Update()
  {
    if (isStreetViewActive && Input.GetKeyDown(KeyCode.Escape))
    {
      ToggleCursorLock();
    }

    if (!cursorLock) return;

    if (canFly)
    {
      if (Input.GetKey(KeyCode.Space))
      {
        rb.velocity = new Vector3(rb.velocity.x, 5, rb.velocity.y);
      }
      else if (Input.GetKey(KeyCode.LeftControl))
      {
        rb.velocity = new Vector3(rb.velocity.x, -5, rb.velocity.y);
      }
      else
      {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.y);
      }
    }
    Look();
  }

  void FixedUpdate()
  {
    if (!cursorLock) return;

    Movement();
  }

  void ToggleCursorLock()
  {
    if (!cursorLock)
    {
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
      cursorLock = true;
    }
    else
    {
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