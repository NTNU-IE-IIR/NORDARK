using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    private Camera sceneCamera;
    private LightPolesManager lightPolesManager;
    private VegetationManager vegetationManager;
    private int configurationIndex;

    void Awake()
    {
        sceneCamera = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDestroy()
    {
        if (vegetationManager != null) {
            vegetationManager.RemoveCamera(sceneCamera);
        }
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == sceneCamera && lightPolesManager != null) {
            List<LightPole> mainCameraLights = lightPolesManager.GetLights();

            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.Light.ShowLight(lightPole.ConfigurationIndex == configurationIndex);
            }
        }
    }

    public void Create(Rect viewport, LightPolesManager lightPolesManager, VegetationManager vegetationManager, int configurationIndex)
    {
        sceneCamera.rect = viewport;

        vegetationManager.AddCamera(sceneCamera);
        
        this.lightPolesManager = lightPolesManager;
        this.vegetationManager = vegetationManager;
        this.configurationIndex = configurationIndex;
    }
}