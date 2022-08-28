using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPrefab : MonoBehaviour
{
    bool isMoving;
    Vector3 baseScale;
    UnityEngine.Rendering.HighDefinition.HDAdditionalLightData hdAdditionalLightData;
    IESLight iesLight;

    void Awake()
    {
        isMoving = false;
        baseScale = transform.localScale;
        hdAdditionalLightData = transform.Find("Spot Light").gameObject.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
    }

    void Update()
    {
        if (isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 10000, EnvironmentManager.GetEnvironmentLayer()))
            {
                transform.position = hit.point;
            }
        }
    }

    public void Create(LightNode lightNode, Transform parent, Vector3 eulerAngles, EnvironmentManager environmentManager)
    {
        transform.parent = parent;
        transform.position = environmentManager.GetUnityPositionFromCoordinatesAndAltitude(lightNode.LatLong, lightNode.Altitude);
        transform.localScale = baseScale * environmentManager.GetWorldRelativeScale();
        transform.eulerAngles = eulerAngles;
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
}