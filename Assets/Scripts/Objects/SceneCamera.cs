using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    private Camera sceneCamera;
    private LightsManager lightsManager;
    private IEnumerable<LightPrefab> mainCameraLights;

    void Awake()
    {
        sceneCamera = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == sceneCamera && lightsManager != null) {
            mainCameraLights = lightsManager.GetLights();

            foreach (LightPrefab lightPrefab in mainCameraLights) {
                lightPrefab.ShowLight(false);
            }
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == sceneCamera && mainCameraLights != null) {
            foreach (LightPrefab lightPrefab in mainCameraLights) {
                lightPrefab.ShowLight(true);
            }
            mainCameraLights = null;
        }
    }

    public void Create(Rect viewport, LightsManager lightsManager)
    {
        sceneCamera.rect = viewport;
        this.lightsManager = lightsManager;
    }
}