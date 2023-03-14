using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class LightPolesControl : MonoBehaviour
{
    private const string NO_GROUP_MESSAGE = "No group selected";
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightPolesSelectionManager lightPolesSelectionManager;
    [SerializeField] private TMP_InputField selectedLightPoleName;
    [SerializeField] private TMP_Dropdown selectFromGroup;
    [SerializeField] private Slider height;
    [SerializeField] private TMP_Text heightValue;
    [SerializeField] private Slider rotation;
    [SerializeField] private TMP_Text rotationValue;
    [SerializeField] private TMP_Dropdown addToGroupDropdown;
    [SerializeField] private Button addToGroup;
    [SerializeField] private TMP_Dropdown removeFromGroupDropdown;
    [SerializeField] private Button removeFromGroup;
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
        Assert.IsNotNull(selectFromGroup);
        Assert.IsNotNull(height);
        Assert.IsNotNull(heightValue);
        Assert.IsNotNull(rotation);
        Assert.IsNotNull(rotationValue);
        Assert.IsNotNull(addToGroupDropdown);
        Assert.IsNotNull(addToGroup);
        Assert.IsNotNull(removeFromGroupDropdown);
        Assert.IsNotNull(removeFromGroup);
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

        selectFromGroup.onValueChanged.AddListener(value => lightPolesManager.SelectFromGroup(selectFromGroup.options[value].text));

        height.onValueChanged.AddListener(value => {
            lightPolesManager.ChangeSelectedLightPolesHeight(value);
            SetHeight(value);
        });

        rotation.onValueChanged.AddListener(value => {
            lightPolesManager.RotateSelectedLightPoles(value);
            SetRotation(value);
        });

        addToGroup.onClick.AddListener(() => {
            try {
                string group = addToGroupDropdown.options[addToGroupDropdown.value].text;
                lightPolesManager.AddGroupToSelectedLightPoles(group);

                List<string> removeFromGroupList = removeFromGroupDropdown.options.Select(option => option.text).ToList();
                if (!removeFromGroupList.Contains(group)) {
                    removeFromGroupDropdown.AddOptions(new List<string> {group});
                }

                List<string> addToGroupList = addToGroupDropdown.options.Select(option => option.text).ToList();
                addToGroupList.Remove(group);
                addToGroupDropdown.ClearOptions();
                addToGroupDropdown.AddOptions(addToGroupList);
            } catch(System.Exception) {}
        });

        removeFromGroup.onClick.AddListener(() => {
            try {
                string group = removeFromGroupDropdown.options[removeFromGroupDropdown.value].text;
                lightPolesManager.RemoveGroupFromSelectedLightPoles(group);
                
                List<string> addToGroupList = addToGroupDropdown.options.Select(option => option.text).ToList();
                if (!addToGroupList.Contains(group)) {
                    addToGroupDropdown.AddOptions(new List<string> {group});
                }
                
                List<string> removeFromGroupList = removeFromGroupDropdown.options.Select(option => option.text).ToList();
                removeFromGroupList.Remove(group);
                removeFromGroupDropdown.ClearOptions();
                removeFromGroupDropdown.AddOptions(removeFromGroupList);
            } catch(System.Exception) {}
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
            lightPolesManager.SelectLightPolePointerByCursor(Input.GetKey(KeyCode.LeftControl));
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            lightPolesManager.ClearSelectedLightPoles();
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            lightPolesSelectionManager.StartDrawing();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            lightPolesManager.AddLightPole();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            lightPolesManager.MoveSelectedLightPoles();
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            lightPolesManager.DeleteSelectedLightPoles();
        }
    }

    public void ClearSelectedLights()
    {
        selectedLightPoleName.text = "No selection";
        selectedLightPoleName.interactable = false;
        selectFromGroup.SetValueWithoutNotify(0);
        height.value = 0;
        heightValue.text = "";
        rotation.value = 0;
        rotationValue.text = "";
        addToGroupDropdown.ClearOptions();
        removeFromGroupDropdown.ClearOptions();
    }

    public void LightSelected(LightPole selectedLightPole, bool multipleSelectedLightPoles)
    {
        float heightSelected = selectedLightPole.Light.GetHeight();
        float rotationSelected = System.Math.Max(0, selectedLightPole.Light.transform.eulerAngles.y);
        int iesFileIndex = lightIESFile.options.FindIndex(i => i.text.Equals(selectedLightPole.Light.GetIESLight().Name));
        int prefabIndex = light3DModel.options.FindIndex(i => i.text.Equals(selectedLightPole.PrefabName));
        
        selectedLightPoleName.text = multipleSelectedLightPoles ? "Multiple light poles selected" : selectedLightPole.Name;
        selectedLightPoleName.interactable = !multipleSelectedLightPoles;
        if (!selectedLightPole.Groups.Contains(selectFromGroup.options[selectFromGroup.value].text)) {
            selectFromGroup.SetValueWithoutNotify(0);
        }
        
        height.value = heightSelected;
        SetHeight(heightSelected);
        rotation.value = rotationSelected;
        SetRotation(rotationSelected);

        List<string> addToGroupList = addToGroupDropdown.options.Select(option => option.text).ToList();
        List<string> removeFromGroupList = removeFromGroupDropdown.options.Select(option => option.text).ToList();
        foreach (TMP_Dropdown.OptionData optionData in selectFromGroup.options) {
            if (optionData.text != NO_GROUP_MESSAGE) {
                if (selectedLightPole.Groups.Contains(optionData.text)) {
                    if (!removeFromGroupList.Contains(optionData.text)) {
                        removeFromGroupDropdown.AddOptions(new List<string> {optionData.text});
                    }
                } else {
                    if (!addToGroupList.Contains(optionData.text)) {
                        addToGroupDropdown.AddOptions(new List<string> {optionData.text});
                    }
                }
            }
        }
        
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

    public void SetGroups(List<string> groups)
    {
        groups.Insert(0, NO_GROUP_MESSAGE);
        selectFromGroup.ClearOptions();
        selectFromGroup.AddOptions(groups);
    }

    public void SetCurrentGroup(string group)
    {
        int index = selectFromGroup.options.Select(option => option.text).ToList().IndexOf(group);
        if (index > -1) {
            selectFromGroup.SetValueWithoutNotify(index);
        }
    }

    public bool IsHighlighted()
    {
        return highlight.isOn;
    }

    private void SetHeight(float height)
    {
        heightValue.text = height.ToString("0.00") + "m";
    }

    private void SetRotation(float rotation)
    {
        rotationValue.text = rotation.ToString("0.00") + "°";
    }
}