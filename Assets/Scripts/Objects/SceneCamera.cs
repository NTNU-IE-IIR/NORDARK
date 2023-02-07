using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    private Camera sceneCamera;
    private LightsManager lightsManager;
    private List<LightPole> mainCameraLights;
    private int configurationIndex;

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

            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.Light.ShowLight(lightPole.ConfigurationIndex == configurationIndex);
            }
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == sceneCamera && mainCameraLights != null) {
            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.Light.ShowLight(lightPole.ConfigurationIndex == 0);
            }
            mainCameraLights = null;
        }
    }

    public void Create(Rect viewport, LightsManager lightsManager, int configurationIndex)
    {
        sceneCamera.rect = viewport;
        this.lightsManager = lightsManager;
        this.configurationIndex = configurationIndex;
    }
}