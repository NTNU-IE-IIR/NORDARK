using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    private Camera sceneCamera;
    private LightsManager lightsManager;
    private VegetationManager vegetationManager;
    private int configurationIndex;

    void Awake()
    {
        sceneCamera = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDestroy()
    {
        vegetationManager.RemoveCamera(sceneCamera);
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == sceneCamera && lightsManager != null) {
            List<LightPole> mainCameraLights = lightsManager.GetLights();

            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.Light.ShowLight(lightPole.ConfigurationIndex == configurationIndex);
            }
        }
    }

    public void Create(Rect viewport, bool orthographic, float orthographicSize, LightsManager lightsManager, VegetationManager vegetationManager, int configurationIndex)
    {
        sceneCamera.rect = viewport;
        sceneCamera.orthographic = orthographic;
        if (sceneCamera.orthographic) {
            sceneCamera.orthographicSize = orthographicSize;
        }

        vegetationManager.AddCamera(sceneCamera);
        
        this.lightsManager = lightsManager;
        this.vegetationManager = vegetationManager;
        this.configurationIndex = configurationIndex;
    }
}