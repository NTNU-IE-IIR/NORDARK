using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class Minimap : MonoBehaviour
{
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private LocationsManager locationsManager;
    [SerializeField] private MinimapUI minimapUI;
    private Camera unityCamera;
    private float defaultOrthographicSize;

    void Awake()
    {
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(minimapUI);

        unityCamera = GetComponent<Camera>();
        defaultOrthographicSize = unityCamera.orthographicSize;
    }

    // LateUpdate and not Update because the unityCamera euler angles must be changed after those of the main camera
    void LateUpdate()
    {
        if (unityCamera.orthographic) {
            unityCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            unityCamera.orthographicSize = defaultOrthographicSize * locationsManager.GetCurrentTerrainMultiplier();
        } else {
            unityCamera.transform.eulerAngles = new Vector3(60, 0, 0);
        }
    }

    public void ChangeViewType()
    {
        unityCamera.orthographic = !unityCamera.orthographic;
        sceneCamerasManager.ChangeViewType();
    }

    public void Show(bool show)
    {
        minimapUI.gameObject.SetActive(show);
        gameObject.SetActive(show);
    }
}
