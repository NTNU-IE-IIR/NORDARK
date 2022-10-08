using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPrefab : MonoBehaviour
{
    private const float LIGHT_TEMPERATURE = 6500;

    bool isMoving;
    Vector3 baseScale;
    public UnityEngine.Rendering.HighDefinition.HDAdditionalLightData hdAdditionalLightData;
    IESLight iesLight;
    Renderer objectRenderer;
    Material defaultMaterial;

    void Awake()
    {
        GameObject lightObject = transform.Find("Light").gameObject;

        isMoving = false;
        baseScale = transform.localScale;
        hdAdditionalLightData = lightObject.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
        objectRenderer = GetComponent<Renderer>();
        defaultMaterial = objectRenderer.material;
    }

    void Update()
    {
        if (isMoving)
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (!isOverUI && Physics.Raycast(ray, out hit, 10000, 1 << MapManager.UNITY_LAYER_MAP))
            {
                transform.position = hit.point;
            }
        }
    }

    public void Create(LightNode lightNode, Transform parent, Vector3 eulerAngles, MapManager mapManager)
    {
        transform.parent = parent;
        transform.position = mapManager.GetUnityPositionFromCoordinatesAndAltitude(lightNode.LatLong, lightNode.Altitude, true);
        transform.localScale = baseScale * mapManager.GetWorldRelativeScale();
        transform.eulerAngles = eulerAngles;
        hdAdditionalLightData.SetColor(hdAdditionalLightData.color, LIGHT_TEMPERATURE);
    }

    public void SetIntensity(float value){
        GameObject lightObject = transform.Find("Light").gameObject;
        Light light = lightObject.GetComponent<Light>();
        light.intensity = value;
    }

    public float GetIntensity(){
        return hdAdditionalLightData.intensity;
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void Rotate(float rotation)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = rotation;
        transform.eulerAngles = angles;
    }

    public void MultiplyScale(float scale)
    {
        transform.localScale = baseScale * scale;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void SetIESLight(IESLight iesLight)
    {
        this.iesLight = iesLight;
        hdAdditionalLightData.SetCookie(iesLight.Cookie);
        hdAdditionalLightData.SetIntensity(iesLight.Intensity.Value, iesLight.Intensity.Unit);
    }

    public void SetLightIntensity(float value){
        hdAdditionalLightData.SetIntensity(value);
    }

    public void SetLightColor(Color color){
        hdAdditionalLightData.SetColor(color);
    }

    public IESLight GetIESLight()
    {
        return iesLight;
    }

    public void Show(bool display) {
        gameObject.SetActive(display);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void Hightlight(bool hightlight, Material highlightMaterial)
    {
        if (hightlight) {
            objectRenderer.material = highlightMaterial;
        } else {
            objectRenderer.material = defaultMaterial;
        }
    }
}