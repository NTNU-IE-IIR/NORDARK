using UnityEngine;

[RequireComponent(typeof(SceneCamerasManager))]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float lookSpeedH = 5f;
    [SerializeField] private float lookSpeedV = 5f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float dragSpeed = 5f;
    [SerializeField] private float dragSpeedOrthographic = 5f;
    [SerializeField] private float dragSpeedY = 5f;
    private SceneCamerasManager sceneCamerasManager;
    private float yaw;
    private float pitch;
    private bool orthographic;

    void Start()
    {
        sceneCamerasManager = GetComponent<SceneCamerasManager>();
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        orthographic = false;
    }

    void Update()
    {
        KeyboardMovement();
        MouseRotation();
    }
    
    public void SetOrthographic(bool orthographic)
    {
        this.orthographic = orthographic;
    }

    private void KeyboardMovement()
    {
        if (orthographic) {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            transform.Translate(new Vector3(0, -this.dragSpeedOrthographic * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                transform.Translate(new Vector3(0, this.dragSpeedOrthographic * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.Q)) {
                transform.Translate(new Vector3(0, 0, -this.dragSpeedY * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.E)) {
                transform.Translate(new Vector3(0, 0, this.dragSpeedY * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                transform.Translate(new Vector3(this.dragSpeedOrthographic * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                transform.Translate(new Vector3(-this.dragSpeedOrthographic * Time.deltaTime, 0, 0));
            }
        } else {
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            transform.Translate(new Vector3(0, 0, -this.dragSpeed * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                transform.Translate(new Vector3(0, 0, this.dragSpeed * Time.deltaTime));
            }
            if (Input.GetKey(KeyCode.Q)) {
                transform.Translate(new Vector3(0, -this.dragSpeedY * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.E)) {
                transform.Translate(new Vector3(0, this.dragSpeedY * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                transform.Translate(new Vector3(this.dragSpeed * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                transform.Translate(new Vector3(-this.dragSpeed * Time.deltaTime, 0, 0));
            }
        }
    }

    private void MouseRotation()
    {
        if (orthographic) {
            if (Input.GetMouseButton(2)) {
                transform.Translate(
                    -Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeedOrthographic,
                    -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeedOrthographic,
                    0
                );
            }

            sceneCamerasManager.IncreaseCameraSize(-Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);
        } else {
            if (Input.GetMouseButton(1)) {
                yaw += lookSpeedH * Input.GetAxis("Mouse X");
                pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

                transform.eulerAngles = new Vector3(pitch, yaw, 0f);
            }

            if (Input.GetMouseButton(2)) {
                transform.Translate(
                    -Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed,
                    -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed,
                    0
                );
            }

            transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
        }   
    }
}
