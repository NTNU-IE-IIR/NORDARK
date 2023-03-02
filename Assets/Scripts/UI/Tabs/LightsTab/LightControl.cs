using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class LightControl : MonoBehaviour
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightPolesSelectionManager lightPolesSelectionManager;
    [SerializeField] private TMP_InputField selectedLightPoleName;
    [SerializeField] private Slider rotation;
    [SerializeField] private TMP_Text rotationValue;
    [SerializeField] private TMP_Dropdown lightIESFile;
    [SerializeField] private TMP_Dropdown light3DModel;
    [SerializeField] private Button select;
    [SerializeField] private Button add;
    [SerializeField] private Button move;
    [SerializeField] private Button delete;
    [SerializeField] private Toggle highlight;
    [SerializeField] private Toggle display;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(lightPolesSelectionManager);
        Assert.IsNotNull(selectedLightPoleName);
        Assert.IsNotNull(rotation);
        Assert.IsNotNull(rotationValue);
        Assert.IsNotNull(lightIESFile);
        Assert.IsNotNull(light3DModel);
        Assert.IsNotNull(select);
        Assert.IsNotNull(add);
        Assert.IsNotNull(move);
        Assert.IsNotNull(delete);
        Assert.IsNotNull(highlight);
        Assert.IsNotNull(display);
    }

    void Start()
    {
        selectedLightPoleName.onEndEdit.AddListener(lightPolesManager.ChangeSelectedLightPolesName);

        rotation.onValueChanged.AddListener(value => {
            lightPolesManager.RotateSelectedLightPoles(value);
            rotationValue.text = value.ToString();
        });

        lightIESFile.onValueChanged.AddListener(value => {
            lightPolesManager.ChangeIESFileOfSelectedLightPoles(lightIESFile.options[value].text);
        });

        light3DModel.AddOptions(lightPolesManager.GetLightPrefabNames());
        light3DModel.onValueChanged.AddListener(value => {
            lightPolesManager.Change3DModelOfSelectedLightPoles(light3DModel.options[value].text);
        });

        select.onClick.AddListener(lightPolesSelectionManager.StartDrawing);
        add.onClick.AddListener(lightPolesManager.AddLightPole);
        move.onClick.AddListener(lightPolesManager.MoveSelectedLightPoles);
        delete.onClick.AddListener(() => {
            lightPolesManager.DeleteSelectedLightPoles();
            EventSystem.current.SetSelectedGameObject(null);
        });

        highlight.onValueChanged.AddListener(lightPolesManager.HighlightLights);
        display.onValueChanged.AddListener(lightPolesManager.ShowLightPoles);
    }

    void Update()
    {   
        if (Input.GetMouseButtonDown(0)) {
            lightPolesManager.SelectLightPoleFromPointerByCursor(Input.GetKey(KeyCode.LeftControl));
        } else if (Input.GetMouseButtonDown(1)) {
            lightPolesManager.ClearSelectedLightPoles();
        }
    }

    public void ClearSelectedLight()
    {
        selectedLightPoleName.text = "No selection";
        rotation.value = 0;
        rotationValue.text = "";
    }

    public void LightSelected(LightPole selectedLightPole, bool multipleSelectedLightPoles)
    {
        float rotationSelected = System.Math.Max(0, selectedLightPole.Light.transform.eulerAngles.y);
        int iesFileIndex = lightIESFile.options.FindIndex(i => i.text.Equals(selectedLightPole.Light.GetIESLight().Name));
        int prefabIndex = light3DModel.options.FindIndex(i => i.text.Equals(selectedLightPole.PrefabName));

        selectedLightPoleName.text = multipleSelectedLightPoles ? "Multiple light poles selected" : selectedLightPole.Name;
        rotation.value = rotationSelected;
        rotationValue.text = rotationSelected.ToString();
        
        if (iesFileIndex >= 0) {
            lightIESFile.value = iesFileIndex;
        }
        if (prefabIndex >= 0) {
            light3DModel.value = prefabIndex;
        }
    }

    public void SetIESNames(List<string> iesNames)
    {
        lightIESFile.ClearOptions();
        lightIESFile.AddOptions(iesNames);
    }

    public bool IsHighlighted()
    {
        return highlight.isOn;
    }
}