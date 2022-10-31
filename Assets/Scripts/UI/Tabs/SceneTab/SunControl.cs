using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class SunControl : MonoBehaviour
{
    [SerializeField]
    private SunManager sunManager;
    [SerializeField]
    private TMP_Dropdown month;
    [SerializeField]
    private TMP_InputField year;
    [SerializeField]
    private TMP_Dropdown hour;
    [SerializeField]
    private TMP_Dropdown minute;

    void Awake()
    {
        Assert.IsNotNull(sunManager);
        Assert.IsNotNull(month);
        Assert.IsNotNull(year);
        Assert.IsNotNull(hour);
        Assert.IsNotNull(minute);

        List<string> hours = new List<string>();
        for (int i=0; i<24; i++) {
            hours.Add(i.ToString());
        }
        hour.AddOptions(hours);

        List<string> minutes = new List<string>();
        for (int i=0; i<60; i++) {
            minutes.Add(i.ToString());
        }
        minute.AddOptions(minutes);
    }

    void Start()
    {
        month.onValueChanged.AddListener(change => ChangeDateTime());
        year.onEndEdit.AddListener(change => ChangeDateTime());
        hour.onValueChanged.AddListener(change => ChangeDateTime());
        minute.onValueChanged.AddListener(change => ChangeDateTime());
    }

    public void SetUpUI()
    {
        System.DateTime sceneDateTime = sunManager.GetCurrentDateTime();
        month.value = sceneDateTime.Month-1;
        year.text = sceneDateTime.Year.ToString();
        hour.value = sceneDateTime.Hour;
        minute.value = sceneDateTime.Minute;
    }

    private void ChangeDateTime()
    {
        sunManager.SetCurrentDateTime(new System.DateTime(System.Int32.Parse(year.text), month.value+1, 1, hour.value, minute.value, 0));
    }
}