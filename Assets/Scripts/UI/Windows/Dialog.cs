using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class Dialog : MonoBehaviour
{
    [SerializeField] private Button close;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Button OK;

    void Awake()
    {
        Assert.IsNotNull(close);
        Assert.IsNotNull(text);
        Assert.IsNotNull(OK);
    }
    
    void Start()
    {
        close.onClick.AddListener(() => {
            Destroy(gameObject);
        });
        OK.onClick.AddListener(() => {
            Destroy(gameObject);
        });
    }

    public void SetMessage(string message)
    {
        text.text = message;
    }
}
