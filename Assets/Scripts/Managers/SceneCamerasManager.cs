using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera), typeof(CameraMovement))]
public class SceneCamerasManager : MonoBehaviour
{
    public const int HIDDEN_FROM_CAMERA_LAYER = 12;
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private VegetationManager vegetationManager;
    [SerializeField] private GameObject sceneCameraPrefab;
    private Camera mainCamera;
    private CameraMovement cameraMovement;
    private Rect viewportRect;
    private int numberOfScreens;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(sceneCameraPrefab);

        mainCamera = GetComponent<Camera>();
        cameraMovement = GetComponent<CameraMovement>();
        viewportRect = mainCamera.rect;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    public void SplitScreen(int numberOfScreens)
    {
        this.numberOfScreens = numberOfScreens;
        foreach (Transform sceneCamera in transform) {
            Destroy(sceneCamera.gameObject);
        }
        ShowOnlyMainCameraLights();

        mainCamera.rect = new Rect(
            viewportRect.x,
            numberOfScreens > 1 ? viewportRect.y + viewportRect.height * 0.5f : viewportRect.y,
            viewportRect.width * 1f / ((int) ((numberOfScreens + 1) / 2)),
            numberOfScreens > 1 ? viewportRect.height * 0.5f : viewportRect.height
        );
        for (int i=1; i<numberOfScreens; ++i) {
            SceneCamera sceneCamera = Instantiate(sceneCameraPrefab, transform).GetComponent<SceneCamera>();
            sceneCamera.Create(new Rect(
                i < numberOfScreens/2f ? viewportRect.x + mainCamera.rect.width * i : viewportRect.x + mainCamera.rect.width * i - viewportRect.width,
                i < numberOfScreens/2f ? viewportRect.y + viewportRect.height * 0.5f : viewportRect.y,
                i < numberOfScreens/2f ? mainCamera.rect.width : viewportRect.width / (numberOfScreens - i),
                mainCamera.rect.height
            ), mainCamera.orthographic, mainCamera.orthographicSize, lightPolesManager, vegetationManager, i);
        }
    }

    public void ChangeViewType()
    {
        mainCamera.orthographic = !mainCamera.orthographic;
        if (mainCamera.orthographic) {
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        foreach (Transform child in transform) {
            Camera camera = child.GetComponent<Camera>();
            camera.orthographic = mainCamera.orthographic;

            if (camera.orthographic) {
                camera.orthographicSize = mainCamera.orthographicSize;
            }
        }

        cameraMovement.SetOrthographic(mainCamera.orthographic);   
    }

    public bool isView2D()
    {
        return mainCamera.orthographic;
    }

    public void IncreaseCameraSize(float increase)
    {
        if (mainCamera.orthographic) {
            mainCamera.orthographicSize += increase;
            if (mainCamera.orthographicSize <= 0) {
                mainCamera.orthographicSize = 0.1f;
            }

            foreach (Transform child in transform) {
                child.GetComponent<Camera>().orthographicSize = mainCamera.orthographicSize;
            }
        }
    }

    public Ray ScreenPointToRay(Vector3 position)
    {
        Camera camera = GetCameraPointedByPosition(position);
        if (camera == null) {
            return new Ray();
        } else {
            return camera.ScreenPointToRay(position);
        }
    }

    public Vector3 ScreenToWorldPoint(Vector3 position)
    {
        Camera camera = GetCameraPointedByPosition(position);
        if (camera == null) {
            return new Vector3();
        } else {
            return camera.ScreenToWorldPoint(position);
        }
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (numberOfScreens > 1 && camera == mainCamera) {
            ShowOnlyMainCameraLights();
        }
    }

    private void ShowOnlyMainCameraLights()
    {
        List<LightPole> mainCameraLights = lightPolesManager.GetLights();

        foreach (LightPole lightPole in mainCameraLights) {
            lightPole.Light.ShowLight(lightPole.ConfigurationIndex == 0);
        }
    }

    private Camera GetCameraPointedByPosition(Vector3 position)
    {
        Vector2 positionNormalized = new Vector2(position.x / Screen.width, position.y / Screen.height);
        if (mainCamera.rect.Contains(positionNormalized)) {
            return mainCamera;
        }

        foreach (Transform child in transform) {
            Camera camera = child.GetComponent<Camera>();
            if (camera.rect.Contains(positionNormalized)) {
                return camera;
            }
        }

        return null;
    }
}