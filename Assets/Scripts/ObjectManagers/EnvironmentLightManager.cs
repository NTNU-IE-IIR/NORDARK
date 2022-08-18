using UnityEngine;

public class EnvironmentLightManager : MonoBehaviour
{
    private Light environmentLight;

    void Start()
    {
        environmentLight = GetComponent<Light>();
    }

    public void SetIntensity(float intensity)
    {
        environmentLight.intensity = intensity;
    }

    public float GetIntensity()
    {
        return environmentLight.intensity;
    }

    public void SetTemperature(float temperature)
    {
        environmentLight.colorTemperature = temperature;
    }

    public float GetTemperature()
    {
        return environmentLight.colorTemperature;
    }
}
