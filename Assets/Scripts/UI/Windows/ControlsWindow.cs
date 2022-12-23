using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ControlsWindow : MonoBehaviour
{
    [SerializeField] private Button close;

    void Awake()
    {
        Assert.IsNotNull(close);
    }

    void Start()
    {
        close.onClick.AddListener(() => {
            gameObject.SetActive(false);
        });
    }
}
