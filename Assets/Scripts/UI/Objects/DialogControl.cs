using UnityEngine;
using UnityEngine.Assertions;

public class DialogControl : MonoBehaviour
{
    [SerializeField] private GameObject dialogWindow;
    private static DialogControl instance;

    void Awake()
    {
        Assert.IsNotNull(dialogWindow);
        
        instance = this;
    }

    public static void CreateDialog(string message)
    {
        instance.CreateInfoDialog(message);
    }

    private void CreateInfoDialog(string message)
    {
        Dialog dialog = Instantiate(dialogWindow, transform).GetComponent<Dialog>();
        dialog.SetMessage(message);
    }
}