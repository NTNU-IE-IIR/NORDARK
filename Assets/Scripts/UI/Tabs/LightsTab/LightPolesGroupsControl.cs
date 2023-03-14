using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class LightPolesGroupsControl : MonoBehaviour
{
    [SerializeField] private LightPolesGroupsManager lightPolesGroupsManager;
    [SerializeField] private TMP_InputField newGroupName;
    [SerializeField] private Button addGroup;
    [SerializeField] private TMP_Dropdown deleteGroupNames;
    [SerializeField] private Button deleteGroup;

    void Awake()
    {
        Assert.IsNotNull(lightPolesGroupsManager);
        Assert.IsNotNull(newGroupName);
        Assert.IsNotNull(addGroup);
        Assert.IsNotNull(deleteGroupNames);
        Assert.IsNotNull(deleteGroup);
    }

    void Start()
    {
        addGroup.onClick.AddListener(() => {
            lightPolesGroupsManager.AddGroup(newGroupName.text);
            newGroupName.text = "";
        });
        deleteGroup.onClick.AddListener(() => {
            try {
                lightPolesGroupsManager.RemoveGroup(deleteGroupNames.options[deleteGroupNames.value].text);
            } catch(System.Exception) {}
        });
    }

    public void SetGroups(List<string> groups)
    {
        deleteGroupNames.ClearOptions();
        deleteGroupNames.AddOptions(groups);
    }
}
