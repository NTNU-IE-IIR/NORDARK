using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DialogControl : MonoBehaviour
{
    [SerializeField]
    private GameObject dialogWindow;

    void Awake()
    {
        Assert.IsNotNull(dialogWindow);
    }

    public void CreateInfoDialog(string message)
    {
        Dialog dialog = (Instantiate(dialogWindow, transform) as GameObject).GetComponent<Dialog>();
        dialog.SetMessage(message);
    }
}
