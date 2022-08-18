using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ChangeWeather : MonoBehaviour
{
    [SerializeField]
    private Dropdown methodSelection;
    [SerializeField]
    private GameObject sunny;
    [SerializeField]
    private GameObject partlyCloudy;
    [SerializeField]
    private GameObject rainy;
    [SerializeField]
    private GameObject rainyParticle;
    [SerializeField]
    private GameObject snowy;
    [SerializeField]
    private GameObject snowySun;
    [SerializeField]
    private GameObject snowPlane;

    void Awake()
    {
        Assert.IsNotNull(methodSelection);
        Assert.IsNotNull(sunny);
        Assert.IsNotNull(partlyCloudy);
        Assert.IsNotNull(rainy);
        Assert.IsNotNull(rainyParticle);
        Assert.IsNotNull(snowy);
        Assert.IsNotNull(snowySun);
        Assert.IsNotNull(snowPlane);
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
        SetSunny();
    }

    private void SetSunny()
    {
        Clear();
        sunny.SetActive(true);
    }

    private void SetCloudy()
    {
        Clear();
    }

    private void SetPartlyCloudy()
    {
        Clear();
        partlyCloudy.SetActive(true);
    }

    private void SetRainy()
    {
        Clear();
        rainy.SetActive(true);
        rainyParticle.SetActive(true);
    }

    private void SetSnowy()
    {
        Clear();
        snowy.SetActive(true);
        snowySun.SetActive(true);
        snowPlane.SetActive(true);
    }

    private void Clear()
    {
        sunny.SetActive(false);
        partlyCloudy.SetActive(false);
        rainy.SetActive(false);
        rainyParticle.SetActive(false);
        snowy.SetActive(false);
        snowySun.SetActive(false);
        snowPlane.SetActive(false);
    }
}
