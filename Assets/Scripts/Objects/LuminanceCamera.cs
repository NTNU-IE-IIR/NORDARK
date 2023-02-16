using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class LuminanceCamera : MonoBehaviour
{
    private Camera luminanceCamera;
    private VegetationManager vegetationManager;
    private LightsManager lightsManager;
    private int configurationIndex;
    private RenderTexture luminanceTexture;

    void Awake()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        configurationIndex = 0;
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        vegetationManager.RemoveCamera(luminanceCamera);
        luminanceTexture.Release();

        List<LightPole> mainCameraLights = lightsManager.GetLights();
        foreach (LightPole lightPole in mainCameraLights) {
            lightPole.Light.ShowLight(lightPole.ConfigurationIndex == 0);
        }
    }

    public void Create(int maskTextureSize, VegetationManager vegetationManager, LightsManager lightsManager)
    {
        this.vegetationManager = vegetationManager;
        this.lightsManager = lightsManager;

        luminanceTexture = new RenderTexture(maskTextureSize, maskTextureSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        luminanceTexture.enableRandomWrite = true;
        luminanceTexture.Create();

        luminanceCamera = GetComponent<Camera>();
        luminanceCamera.targetTexture = luminanceTexture;
        vegetationManager.AddCamera(luminanceCamera);
    }

    public void SetConfigurationIndex(int configurationIndex)
    {
        this.configurationIndex = configurationIndex;
    }

    public void SetPositionAndAngle(Vector3 position, float angle)
    {
        transform.position = position;
        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            angle
        );
    }

    public RenderTexture GetTexture()
    {
        return luminanceTexture;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {   
        if (luminanceCamera == camera) {
            List<LightPole> mainCameraLights = lightsManager.GetLights();

            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.Light.ShowLight(lightPole.ConfigurationIndex == configurationIndex);
            }
        }
    }
}
