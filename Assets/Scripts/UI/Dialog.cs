using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class Dialog : MonoBehaviour
{
    [SerializeField] private Button close;
    [SerializeField] private Button OK;
    [SerializeField] private TMP_Text text;

    void Awake()
    {
        Assert.IsNotNull(close);
        Assert.IsNotNull(OK);
        Assert.IsNotNull(text);
    }
    
    void Start()
    {
        close.onClick.AddListener(delegate {
            Destroy(gameObject);
        });
        OK.onClick.AddListener(delegate {
            Destroy(gameObject);
        });
    }

    public void SetMessage(string message)
    {
        text.text = message;
    }
}
