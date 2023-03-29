using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Assertions;

public class SkyControl : MonoBehaviour
{
    private const int STARTING_YEAR = 1972;
    private const int ENDING_YEAR = 2050;

    [SerializeField] private SkyManager skyManager;
    [SerializeField] private TMP_Dropdown dayInput;
    [SerializeField] private TMP_Dropdown monthInput;
    [SerializeField] private TMP_Dropdown yearInput;
    [SerializeField] private TMP_Dropdown hourInput;
    [SerializeField] private TMP_Dropdown minuteInput;

    void Awake()
    {
        Assert.IsNotNull(skyManager);
        Assert.IsNotNull(dayInput);
        Assert.IsNotNull(monthInput);
        Assert.IsNotNull(yearInput);
        Assert.IsNotNull(hourInput);
        Assert.IsNotNull(minuteInput);

        List<string> years = new List<string>();
        for (int i=STARTING_YEAR; i<ENDING_YEAR+1; i++) {
            years.Add(i.ToString());
        }
        yearInput.AddOptions(years);

        List<string> hours = new List<string>();
        for (int i=0; i<24; i++) {
            hours.Add(i.ToString());
        }
        hourInput.AddOptions(hours);

        List<string> minutes = new List<string>();
        for (int i=0; i<60; i++) {
            minutes.Add(i.ToString());
        }
        minuteInput.AddOptions(minutes);
    }

    void Start()
    {
        dayInput.onValueChanged.AddListener(change => ChangeDateTime());
        monthInput.onValueChanged.AddListener(change => ChangeDateTime());
        yearInput.onValueChanged.AddListener(change => ChangeDateTime());
        hourInput.onValueChanged.AddListener(change => ChangeDateTime());
        minuteInput.onValueChanged.AddListener(change => ChangeDateTime());
    }

    public void SetUpUI()
    {
        monthInput.value = skyManager.DateTime.Month-1;
        yearInput.value = skyManager.DateTime.Year - STARTING_YEAR;
        hourInput.value = skyManager.DateTime.Hour;
        minuteInput.value = skyManager.DateTime.Minute;
        UpdateDateDropdown(skyManager.DateTime.Day);
    }

    private void ChangeDateTime()
    {
        UpdateDateDropdown(dayInput.value+1);
        if (yearInput.value !=-1 && monthInput.value !=-1 && dayInput.value !=-1 && hourInput.value !=-1 && minuteInput.value !=-1) {
            skyManager.DateTime = new System.DateTime(yearInput.value+STARTING_YEAR, monthInput.value+1, dayInput.value+1, hourInput.value, minuteInput.value, 0);
        }
    }

    private void UpdateDateDropdown(int day)
    {
        List<string> days = new List<string>();
        for (int i=1; i<29; i++) {
            days.Add(i.ToString());
        }
        if (monthInput.value == 1) {
            int year = yearInput.value + STARTING_YEAR;
            bool isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            if (isLeapYear) {
                 days.Add("29");
            }
        } else {
            days.Add("29");
            days.Add("30");
            if (monthInput.value == 0 || monthInput.value == 2 || monthInput.value == 4 || monthInput.value == 6 || monthInput.value == 7 || monthInput.value == 9 || monthInput.value == 11) {
                days.Add("31");
            }
        }
        dayInput.ClearOptions();
        dayInput.AddOptions(days);
        if (day > 0 && day < days.Count+1) {
            dayInput.SetValueWithoutNotify(day-1);
        } else {
            dayInput.SetValueWithoutNotify(0);
        }
    }
}