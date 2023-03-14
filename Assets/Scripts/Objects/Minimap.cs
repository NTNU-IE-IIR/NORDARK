using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class Minimap : MonoBehaviour
{
    private const float CAMERA_2D_HEIGHT = 50;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private MinimapUI minimapUI;
    private Camera unityCamera;

    void Awake()
    {
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(minimapUI);

        unityCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (unityCamera.orthographic) {
            // Get map altitude (stickToGround parameter set to true)
            Vector3 position = mapManager.GetUnityPositionFromCoordinates(mapManager.GetCoordinatesFromUnityPosition(transform.position), true);
            position.y += CAMERA_2D_HEIGHT;
            unityCamera.transform.position = position;

            unityCamera.transform.eulerAngles = new Vector3(90, 0, 0);
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
