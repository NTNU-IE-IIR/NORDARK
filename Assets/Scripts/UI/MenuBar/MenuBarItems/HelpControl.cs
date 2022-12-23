using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class HelpControl : MenuBarItemControl
{
    const string NORDARK_URL = "https://nordark.org/";
    [SerializeField] private Button controls;
    [SerializeField] private Button about;
    [SerializeField] private GameObject controlsWindow;

    void Awake()
    {
        Assert.IsNotNull(controls);
        Assert.IsNotNull(about);
        Assert.IsNotNull(controlsWindow);

        base.SetUp();
    }

    void Start()
    {
        controls.onClick.AddListener(() => {
            controlsWindow.SetActive(true);
            gameObject.SetActive(false);
        });

        about.onClick.AddListener(() => {
            Application.OpenURL(NORDARK_URL);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
