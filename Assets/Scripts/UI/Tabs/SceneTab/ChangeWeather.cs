using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ChangeWeather : MonoBehaviour
{
    [SerializeField]
    private Dropdown methodSelection;

    void Awake()
    {
        Assert.IsNotNull(methodSelection);
    }

    void Start()
    {
        methodSelection.onValueChanged.AddListener(delegate {
            switch (methodSelection.options[methodSelection.value].text){
                case "Sunny":
                    SetSunny();
                    break;
                case "Cloudy":
                    SetCloudy();
                    break;
                case "Partly cloudy":
                    SetPartlyCloudy();
                    break;
                case "Rainy":
                    SetRainy();
                    break;
                case "Snowy":
                    SetSnowy();
                    break;
                default:
                    break;
            }
        });
    }

    private void SetSunny()
    {
        Clear();
    }

    private void SetCloudy()
    {
        Clear();
    }

    private void SetPartlyCloudy()
    {
        Clear();
    }

    private void SetRainy()
    {
        Clear();
    }

    private void SetSnowy()
    {
        Clear();
    }

    private void Clear()
    {
        
    }
}