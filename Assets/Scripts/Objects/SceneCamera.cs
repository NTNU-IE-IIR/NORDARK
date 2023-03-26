using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    private Camera sceneCamera;
    private LightPolesManager lightPolesManager;
    private BiomeAreasManager biomeAreasManager;
    private int configurationIndex;

    void Awake()
    {
        sceneCamera = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDestroy()
    {
        if (biomeAreasManager != null) {
            biomeAreasManager.RemoveCamera(sceneCamera);
        }
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == sceneCamera && lightPolesManager != null) {
            List<LightPole> mainCameraLights = lightPolesManager.GetLightPoles();

            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.GameObject.ShowLight(lightPole.ConfigurationIndex == configurationIndex);
            }
        }
    }

    public void Create(Rect viewport, LightPolesManager lightPolesManager, BiomeAreasManager biomeAreasManager, int configurationIndex)
    {
        sceneCamera.rect = viewport;

        biomeAreasManager.AddCamera(sceneCamera);
        
        this.lightPolesManager = lightPolesManager;
        this.biomeAreasManager = biomeAreasManager;
        this.configurationIndex = configurationIndex;
    }
}