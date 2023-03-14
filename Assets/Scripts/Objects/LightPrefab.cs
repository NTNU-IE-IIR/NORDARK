using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Renderer))]
public class LightPrefab : MonoBehaviour
{
    public const float DEFAULT_HEIGHT = 5;
    [SerializeField] private Light unityLight;
    private const float LIGHT_TEMPERATURE = 6500;
    private bool isMoving;
    private UnityEngine.Rendering.HighDefinition.HDAdditionalLightData hdAdditionalLightData;
    private IESLight iesLight;
    private Renderer objectRenderer;
    private Material defaultMaterial;
    private SceneCamerasManager sceneCamerasManager;
    private int defaultLayer;
    private MeshFilter meshFilter;

    void Awake()
    {
        Assert.IsNotNull(unityLight);

        isMoving = false;
        hdAdditionalLightData = unityLight.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
        objectRenderer = GetComponent<Renderer>();
        defaultMaterial = objectRenderer.material;
        defaultLayer = gameObject.layer;
        meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {
        if (isMoving) {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            Ray ray = sceneCamerasManager.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (!isOverUI && Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                transform.position = hit.point;
            }
        }
    }

    public void Create(SceneCamerasManager sceneCamerasManager)
    {
        this.sceneCamerasManager = sceneCamerasManager;
        hdAdditionalLightData.SetColor(hdAdditionalLightData.color, LIGHT_TEMPERATURE);
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public float GetHeight()
    {
        return (meshFilter.mesh.bounds.center.y + meshFilter.mesh.bounds.extents.y) * transform.localScale.y;
    }

    public void SetHeight(float height)
    {
        transform.localScale = new Vector3(
            transform.localScale.x,
            height / (meshFilter.mesh.bounds.center.y + meshFilter.mesh.bounds.extents.y),
            transform.localScale.z
        );
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void Rotate(float rotation)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = rotation;
        transform.eulerAngles = angles;
    }

    public void SetIESLight(IESLight iesLight)
    {
        this.iesLight = iesLight;
        hdAdditionalLightData.SetCookie(iesLight.Cookie);
        hdAdditionalLightData.SetIntensity(iesLight.Intensity.Value, iesLight.Intensity.Unit);
    }

    public IESLight GetIESLight()
    {
        return iesLight;
    }

    public void Show(bool show) {
        gameObject.SetActive(show);
    }

    public void ShowLight(bool display) {
        if (gameObject.activeSelf) {
            
            // gameObject.SetActive() cannot be used here, otherwise a Unity error occurs
            unityLight.enabled = display;

            if (display) {
                gameObject.layer = defaultLayer;
            } else {
                gameObject.layer = SceneCamerasManager.HIDDEN_FROM_CAMERA_LAYER;
            }
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void Highlight(bool hightlight, Material highlightMaterial)
    {
        if (hightlight) {
            objectRenderer.material = highlightMaterial;
        } else {
            objectRenderer.material = defaultMaterial;
        }
    }
}