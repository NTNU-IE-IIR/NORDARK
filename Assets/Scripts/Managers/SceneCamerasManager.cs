using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

[RequireComponent(typeof(Camera), typeof(CameraMovement))]
public class SceneCamerasManager : MonoBehaviour
{
    public const int HIDDEN_FROM_CAMERA_LAYER = 12;
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private VegetationManager vegetationManager;
    [SerializeField] private LuminanceMapManager luminanceMapManager;
    [SerializeField] private Transform additionalSceneCamerasContainer;
    private Camera mainCamera;
    private CameraMovement cameraMovement;
    private Rect viewportRect;
    private int numberOfScreens;
    private Camera luminanceMapCamera;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(luminanceMapManager);
        Assert.IsNotNull(additionalSceneCamerasContainer);

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
        foreach (Transform sceneCamera in additionalSceneCamerasContainer) {
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
            CreateSceneCamera(additionalSceneCamerasContainer).Create(new Rect(
                i < numberOfScreens/2f ? viewportRect.x + mainCamera.rect.width * i : viewportRect.x + mainCamera.rect.width * i - viewportRect.width,
                i < numberOfScreens/2f ? viewportRect.y + viewportRect.height * 0.5f : viewportRect.y,
                i < numberOfScreens/2f ? mainCamera.rect.width : viewportRect.width / (numberOfScreens - i),
                mainCamera.rect.height
            ), lightPolesManager, vegetationManager, i);
        }

        if (luminanceMapCamera != null) {
            luminanceMapCamera.rect = mainCamera.rect;
        }
    }

    public void ChangeViewType()
    {
        mainCamera.orthographic = !mainCamera.orthographic;
        if (mainCamera.orthographic) {
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }

        foreach (Transform child in additionalSceneCamerasContainer) {
            Camera camera = child.GetComponent<Camera>();
            camera.orthographic = mainCamera.orthographic;
            if (camera.orthographic) {
                camera.orthographicSize = mainCamera.orthographicSize;
            }
        }

        if (luminanceMapCamera != null) {
            luminanceMapCamera.orthographic = mainCamera.orthographic;
            if (luminanceMapCamera.orthographic) {
                luminanceMapCamera.orthographicSize = mainCamera.orthographicSize;
            }
        }

        cameraMovement.SetOrthographic(mainCamera.orthographic);   
    }

    public bool IsView2D()
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

            foreach (Transform child in additionalSceneCamerasContainer) {
                child.GetComponent<Camera>().orthographicSize = mainCamera.orthographicSize;
            }

            if (luminanceMapCamera != null) {
                luminanceMapCamera.orthographicSize = mainCamera.orthographicSize;
            }
        }
    }

    public void CreateOrDeleteLuminanceMapCamera(bool create)
    {
        if (luminanceMapCamera != null) {
            luminanceMapCamera.targetTexture.Release();
            Destroy(luminanceMapCamera.gameObject);
            luminanceMapCamera = null;
        }

        if (create) {
            SceneCamera luminanceMapSceneCamera = CreateSceneCamera(transform);
            luminanceMapSceneCamera.Create(
                mainCamera.rect,
                lightPolesManager,
                vegetationManager,
                0
            );
            HDAdditionalCameraData luminanceMapHDCamera = luminanceMapSceneCamera.GetComponent<HDAdditionalCameraData>();
            luminanceMapCamera = luminanceMapSceneCamera.GetComponent<Camera>();

            // See LuminanceCamera.Create() for an explanation of the parameters
            luminanceMapCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            
            luminanceMapHDCamera.volumeLayerMask = 1 << 0 | 1 << LuminanceMapManager.LUMINANCE_IN_LUMINANCE_MAP_LAYER;

            luminanceMapManager.AddCameraToLuminancePass(luminanceMapCamera);
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

    public Color[] GetPixelColorsOfLuminanceMapCameraPointedAroundCursor(int numberOfPixelsAroundCenter)
    {
        bool isCursorOnCameraScreen =
            Input.mousePosition.x > viewportRect.xMin * Screen.width &&
            Input.mousePosition.x < viewportRect.xMax * Screen.width &&
            Input.mousePosition.y > viewportRect.yMin * Screen.height &&
            Input.mousePosition.y < viewportRect.yMax * Screen.height
        ;

        if (isCursorOnCameraScreen) {
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = luminanceMapCamera.targetTexture;

            Texture2D pixelTexture = new Texture2D(numberOfPixelsAroundCenter+1, numberOfPixelsAroundCenter+1);

            pixelTexture.ReadPixels(new Rect(
                Mathf.Min(
                    Mathf.Max(Input.mousePosition.x - numberOfPixelsAroundCenter/2, viewportRect.xMin * Screen.width),
                    viewportRect.xMax * Screen.width - numberOfPixelsAroundCenter
                ),
                Mathf.Min(
                    Mathf.Max(Screen.height - Input.mousePosition.y - numberOfPixelsAroundCenter/2, viewportRect.yMin * Screen.height),
                    viewportRect.yMax * Screen.height - numberOfPixelsAroundCenter
                ),
                numberOfPixelsAroundCenter+1,
                numberOfPixelsAroundCenter+1
            ), 0, 0);

            Color[] colors = pixelTexture.GetPixels(0, 0, numberOfPixelsAroundCenter+1, numberOfPixelsAroundCenter+1);
            Destroy(pixelTexture);

            RenderTexture.active = old;
            return colors;
        } else {
            return null;
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

    private SceneCamera CreateSceneCamera(Transform parent)
    {
        GameObject sceneCameraObject = Instantiate(gameObject, parent);
        sceneCameraObject.transform.localPosition = new Vector3();
        sceneCameraObject.transform.localRotation = Quaternion.identity;
        Destroy(sceneCameraObject.GetComponent<SceneCamerasManager>());
        Destroy(sceneCameraObject.GetComponent<CameraMovement>());
        Destroy(sceneCameraObject.GetComponent<AudioListener>());
        foreach (Transform child in sceneCameraObject.transform) {
            Destroy(child.gameObject);
        }

        return sceneCameraObject.AddComponent<SceneCamera>();
    }

    private Camera GetCameraPointedByPosition(Vector3 position)
    {
        Vector2 positionNormalized = new Vector2(position.x / Screen.width, position.y / Screen.height);
        if (mainCamera.rect.Contains(positionNormalized)) {
            return mainCamera;
        }

        foreach (Transform child in additionalSceneCamerasContainer) {
            Camera camera = child.GetComponent<Camera>();
            if (camera.rect.Contains(positionNormalized)) {
                return camera;
            }
        }

        return null;
    }
}