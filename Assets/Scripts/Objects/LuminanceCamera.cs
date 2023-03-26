using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class LuminanceCamera : MonoBehaviour
{
    private Camera luminanceCamera;
    private BiomeAreasManager biomeAreasManager;
    private LightPolesManager lightPolesManager;
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
        biomeAreasManager.RemoveCamera(luminanceCamera);
        luminanceTexture.Release();

        List<LightPole> mainCameraLights = lightPolesManager.GetLightPoles();
        foreach (LightPole lightPole in mainCameraLights) {
            lightPole.GameObject.ShowLight(lightPole.ConfigurationIndex == 0);
        }
    }

    public void Create(int maskTextureSize, BiomeAreasManager biomeAreasManager, LightPolesManager lightPolesManager)
    {
        this.biomeAreasManager = biomeAreasManager;
        this.lightPolesManager = lightPolesManager;

        // ARGBFloat because we want the highest possible precision, Linear because we want a linear color space (that gives the "true" values of colors)
        luminanceTexture = new RenderTexture(maskTextureSize, maskTextureSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        luminanceTexture.enableRandomWrite = true;
        luminanceTexture.Create();

        luminanceCamera = GetComponent<Camera>();
        luminanceCamera.targetTexture = luminanceTexture;
        biomeAreasManager.AddCamera(luminanceCamera);
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
            List<LightPole> mainCameraLights = lightPolesManager.GetLightPoles();

            foreach (LightPole lightPole in mainCameraLights) {
                lightPole.GameObject.ShowLight(lightPole.ConfigurationIndex == configurationIndex);
            }
        }
    }
}
