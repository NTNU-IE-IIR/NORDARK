using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Character
{
  public float yScale;
  public Character(float yScale)
  {
    this.yScale = yScale;
  }

}

public class CharacterPreset
{
  public static readonly Dictionary<string, Character> defaultSpawnMap = new(){
    {"Child", new Character(1f)},
    {"Adult", new Character(1.5f)},
    {"UFO", new Character(1.5f)}
  };
}

public class SpawnLocation
{
  public static readonly Dictionary<string, Vector3> defaultSpawnMap = new(){
    {"Ålesund", new Vector3(402, 52, 41)}
  };
}

public class StreetViewManager : MonoBehaviour
{
  // TODO: This should be changed - high coupling)
  [SerializeField]
  private TMP_Dropdown locationDropdown;
  [SerializeField]
  private TMP_Dropdown presetDropdown;
  public GameObject mainCamera;
  public GameObject FPSCharacter;
  private GameObject FPSCamera;

  public float walkSpeed = 4.0f;
  public float sensitivity = 5.0f;
  private float yaw = 0.0f;
  private float pitch = 0.0f;
  private Rigidbody rb;
  private bool canFly = false;

  public void DisplayCameraPreview(bool shouldDisplay)
  {
    if (shouldDisplay)
    {
      // Cursor.lockState = CursorLockMode.Locked;
      mainCamera.SetActive(false);
      FPSCharacter.SetActive(true);

      // Debug.Log("PresetDropdown: " + presetDropdown.options[presetDropdown.value].text);

      switch (locationDropdown.options[locationDropdown.value].text)
      {
        case "Ålesund":
          {
            Debug.Log("SpawnLocation: " + SpawnLocation.defaultSpawnMap["Ålesund"]);
            rb.MovePosition(SpawnLocation.defaultSpawnMap["Ålesund"]);
            break;
          }
      }

      // Hardcoded as of now to "Child"
      rb.transform.localScale = new Vector3(
        rb.transform.localScale.x,
        CharacterPreset.defaultSpawnMap["Child"].yScale,
        rb.transform.localScale.z
      );

      // TODO: List is empty for some odd reason
      // foreach( TMP_Dropdown.OptionData option in presetDropdown.options){
      //   Debug.Log("PresetDropdown: " + option.text);
      // }
    }
    else
    {
      mainCamera.SetActive(true);
      FPSCharacter.SetActive(false);
    }
  }

  public void ChangePreset(string value)
  {
    Vector3 currentPosition = rb.transform.position;
    rb.MovePosition(new Vector3(currentPosition.x, currentPosition.y + 3, currentPosition.z));
    rb.transform.localScale = new Vector3(
      rb.transform.localScale.x,
      CharacterPreset.defaultSpawnMap[value].yScale,
      rb.transform.localScale.z
    );

    if (value == "UFO")
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

  void Start()
  {
    rb = FPSCharacter.GetComponent<Rigidbody>();
    FPSCamera = FPSCharacter.transform.Find("FPS Camera").gameObject;
  }

  void Update()
  {
    if (canFly)
    {
      if (Input.GetKey(KeyCode.Space))
      {
        rb.velocity = new Vector3(rb.velocity.x, 5, rb.velocity.y);
      }

      if (Input.GetKey(KeyCode.LeftControl))
      {
        rb.velocity = new Vector3(rb.velocity.x, -5, rb.velocity.y);
      }
    }
    Look();
  }

  void FixedUpdate()
  {
    Movement();
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
