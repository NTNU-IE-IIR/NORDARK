using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LightPolesGroupsManager : MonoBehaviour
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightPolesControl lightPolesControl;
    [SerializeField] private LightPolesGroupsControl lightPolesGroupsControl;
    private HashSet<string> groups;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(lightPolesControl);
        Assert.IsNotNull(lightPolesGroupsControl);

        groups = new HashSet<string>();
    }

    public void Clear()
    {
        groups.Clear();
        UpdateUIGroups();
    }

    public void AddGroups(List<string> groupsToAdd)
    {
        foreach (string groupToAdd in groupsToAdd) {
            groups.Add(groupToAdd);
        }
        UpdateUIGroups();
    }

    public void AddGroup(string groupToAdd)
    {
        lightPolesManager.ClearSelectedObjects();

        if (groupToAdd != "") {
            groups.Add(groupToAdd);
            UpdateUIGroups();
        }
    }

    public void RemoveGroup(string groupToRemove)
    {
        lightPolesManager.ClearSelectedObjects();
        List<LightPole> lightPoles = lightPolesManager.GetLightPoles();
        
        foreach (LightPole lightPole in lightPoles) {
            lightPole.Groups.Remove(groupToRemove);
        }
        SetGroupsFromLightPoles(lightPoles);
        UpdateUIGroups();
    }

    public void SetGroupsFromLightPoles(List<LightPole> lightPoles)
    {
        groups = lightPoles.Aggregate(new HashSet<string>(), (set, lightPole) => {
            foreach (string group in lightPole.Groups) {
                set.Add(group);
            }
            return set;
        });
        UpdateUIGroups();
    }

    private void UpdateUIGroups()
    {
        lightPolesControl.SetGroups(groups.ToList());
        lightPolesGroupsControl.SetGroups(groups.ToList());
    }
}
