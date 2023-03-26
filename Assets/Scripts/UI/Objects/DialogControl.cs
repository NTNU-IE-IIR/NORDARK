using UnityEngine;
using UnityEngine.Assertions;

public class DialogControl : MonoBehaviour
{
    [SerializeField] private GameObject dialogWindow;
    [SerializeField] private GameObject confirmationDialogWindow;
    private static DialogControl instance;

    void Awake()
    {
        Assert.IsNotNull(dialogWindow);
        Assert.IsNotNull(confirmationDialogWindow);
        
        instance = this;
    }

    public static void CreateDialog(string message)
    {
        instance.CreateDialogNonStatic(message);
    }

    public static void CreateConfirmationDialog(string message, System.Action onYesPressed)
    {
        instance.CreateConfirmationDialogNonStatic(message, onYesPressed);
    }

    private void CreateDialogNonStatic(string message)
    {
        Dialog dialog = Instantiate(dialogWindow, transform).GetComponent<Dialog>();
        dialog.SetMessage(message);
    }

    private void CreateConfirmationDialogNonStatic(string message, System.Action onYesPressed)
    {
        ConfirmationDialog confirmationDialog = Instantiate(confirmationDialogWindow, transform).GetComponent<ConfirmationDialog>();
        confirmationDialog.Create(message, onYesPressed);
    }
}