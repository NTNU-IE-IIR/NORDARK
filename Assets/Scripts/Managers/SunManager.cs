using UnityEngine;

public class SunManager : MonoBehaviour
{
    private Light sun;

    void Awake()
    {
        sun = GetComponent<Light>();
    }

    public void SetIntensity(float intensity)
    {
        sun.intensity = intensity;
    }

    public float GetIntensity()
    {
        return sun.intensity;
    }
}