using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private Button close;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button yes;
    [SerializeField] private Button no;
    private System.Action onYesPressed;

    void Awake()
    {
        Assert.IsNotNull(close);
        Assert.IsNotNull(text);
        Assert.IsNotNull(yes);
        Assert.IsNotNull(no);
    }
    
    void Start()
    {
        close.onClick.AddListener(()=>  {
            Destroy(gameObject);
        });
        yes.onClick.AddListener(() => {
            Destroy(gameObject);
            onYesPressed();
        });
        no.onClick.AddListener(() => {
            Destroy(gameObject);
        });
    }

    public void Create(string message, System.Action onYesPressed)
    {
        text.text = message;
        this.onYesPressed = onYesPressed;
    }
}
