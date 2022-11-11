using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class LightControl : MonoBehaviour
{
    [SerializeField] private LightsManager lightsManager;
    [SerializeField] private IESManager iesManager;
    [SerializeField] private TMP_Dropdown lightType;
    [SerializeField] private Slider rotation;
    [SerializeField] private TMP_Text rotationValue;
    [SerializeField] private TMP_Dropdown lightSource;
    [SerializeField] private TMP_Text lightObjectName;
    [SerializeField] private Button insert;
    [SerializeField] private Button move;
    [SerializeField] private Button delete;
    [SerializeField] private Toggle hightlight;
    [SerializeField] private Toggle display;
    private double t1;
    private double t2;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(lightType);
        Assert.IsNotNull(rotation);
        Assert.IsNotNull(rotationValue);
        Assert.IsNotNull(lightSource);
        Assert.IsNotNull(lightObjectName);
        Assert.IsNotNull(insert);
        Assert.IsNotNull(move);
        Assert.IsNotNull(delete);
        Assert.IsNotNull(hightlight);
        Assert.IsNotNull(display);
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

        lightSource.onValueChanged.AddListener(delegate {
            lightsManager.ChangeLightSource(lightSource.options[lightSource.value].text);
        });

        insert.onClick.AddListener(lightsManager.InsertLight);
        move.onClick.AddListener(lightsManager.MoveLight);
        delete.onClick.AddListener(delegate {
            lightsManager.DeleteLight();
            EventSystem.current.SetSelectedGameObject(null);
        });

        hightlight.onValueChanged.AddListener(lightsManager.HighlightLights);
        display.onValueChanged.AddListener(lightsManager.ShowLights);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            if (isDoubleClick()) {
                lightsManager.MoveLight();
            } else {
                lightsManager.SelectLight();
            }
        } else if (Input.GetMouseButtonDown(1)) {
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
        float rotationSelected = System.Math.Max(0, selectedLightNode.Light.transform.eulerAngles.y);
        int lightSourceIndex = lightSource.options.FindIndex((i) => { return i.text.Equals(selectedLightNode.Light.GetIESLight().Name); });
        int lightTypeIndex = lightType.options.FindIndex((i) => { return i.text.Equals(selectedLightNode.PrefabName); });

        lightObjectName.text = "Selected: " + selectedLightNode.Name;
        rotation.value = rotationSelected;
        rotationValue.text = rotationSelected.ToString();
        
        if (lightSourceIndex >= 0) {
            lightSource.value = lightSourceIndex;
        }
        if (lightTypeIndex >= 0) {
            lightType.value = lightTypeIndex;
        }
    }

    public void SetIESNames(List<string> iesNames)
    {
        lightSource.ClearOptions();
        lightSource.AddOptions(iesNames);
    }

    private bool isDoubleClick()
    {
        t2 = Time.realtimeSinceStartup;
        bool doubleClick = t2 - t1 < 0.5f;
        t1 = t2;
        return doubleClick;
    }
}