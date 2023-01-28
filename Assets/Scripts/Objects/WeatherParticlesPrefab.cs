using UnityEngine;

public class WeatherParticlesPrefab : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 initialPosition;

    void Awake()
    {
        cameraTransform = Camera.main.transform;
        initialPosition = transform.position;
    }

    void Update()
    {
        transform.position = initialPosition + cameraTransform.position;
    }
}