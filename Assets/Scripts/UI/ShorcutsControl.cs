using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ShorcutsControl : MonoBehaviour
{
    [SerializeField]
    private LightsManager lightsManager;
    [SerializeField]
    private GameObject mainMenu;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(mainMenu);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) {
            // Help
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            mainMenu.SetActive(!mainMenu.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.F3)) {
            // Screenshot
        }
        if (Input.GetKeyDown(KeyCode.F4)) {
            // 
        }
        if (Input.GetKeyDown(KeyCode.F5)) {
            // Compute
        }
        if (Input.GetKeyDown(KeyCode.F6)) {
            // Save
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            lightsManager.DeleteLight();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            lightsManager.MoveLight();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            lightsManager.InsertLight();
        }
        if (Input.GetKeyDown(KeyCode.Insert)) {
            lightsManager.InsertLight();
        }
    }
}
