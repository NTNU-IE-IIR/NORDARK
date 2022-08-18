using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class LightControl : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private Dropdown lightType;
    [SerializeField]
    private Slider rotation;
    [SerializeField]
    private Text rotationValue;
    [SerializeField]
    private Dropdown lightSource;
    [SerializeField]
    private Button move;
    [SerializeField]
    private Text lightObjectName;
    [SerializeField]
    private Button delete;

    private double t1;
    private double t2;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(lightType);
        Assert.IsNotNull(rotation);
        Assert.IsNotNull(rotationValue);
        Assert.IsNotNull(lightSource);
        Assert.IsNotNull(move);
        Assert.IsNotNull(lightObjectName);
        Assert.IsNotNull(delete);
    }

    void Start()
    {
        lightType.AddOptions(lightsManager.GetLightPrefabNames());
        lightType.onValueChanged.AddListener(delegate {
            lightsManager.ChangeLightType(lightType.options[lightType.value].text);
        });

        rotation.onValueChanged.AddListener(delegate
        {
            lightsManager.RotateSelectedLight(rotation.value);
            rotationValue.text = rotation.value.ToString();
        });

        lightSource.AddOptions(lightsManager.GetIESNames());
        lightSource.onValueChanged.AddListener(delegate {
            lightsManager.ChangeLightSource(lightSource.options[lightSource.value].text);
        });

        move.onClick.AddListener(delegate { lightsManager.MoveLight(); });

        delete.onClick.AddListener(delegate { lightsManager.DeleteLight(); });
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            t2 = Time.realtimeSinceStartup;
            if (t2 - t1 < 0.5f) //<0.5s, considered double click
            {
                lightsManager.MoveLight();
            }
            else
            {
                lightsManager.SelectLight();
            }
            t1 = t2;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            lightsManager.ClearSelectedLight();
        }
    }

    public void ClearSelectedLight()
    {
        lightObjectName.text = "No selection";
        rotation.value = 0;
        rotationValue.text = "";
    }

    public void LightSelected(LightNode selectedLightNode)
    {
        string lightName = selectedLightNode.Light.name;
        float rotationSelected = System.Math.Max(0, selectedLightNode.Light.transform.eulerAngles.y);
        int lightSourceIndex = lightSource.options.FindIndex((i) => { return i.text.Equals(selectedLightNode.IESLight.Name); });
        int lightTypeIndex = lightType.options.FindIndex((i) => { return i.text.Equals(selectedLightNode.PrefabName); });

        lightObjectName.text = "Selected: " + lightName;
        rotation.value = rotationSelected;
        rotationValue.text = rotationSelected.ToString();
        
        if (lightSourceIndex >= 0) {
            lightSource.value = lightSourceIndex;
        }
        if (lightTypeIndex >= 0) {
            lightType.value = lightTypeIndex;
        }
    }
}
