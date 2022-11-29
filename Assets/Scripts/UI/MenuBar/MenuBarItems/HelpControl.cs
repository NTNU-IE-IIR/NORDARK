using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class HelpControl : MenuBarItemControl
{
    const string NORDARK_URL = "https://nordark.org/";
    [SerializeField] private Button about;

    void Awake()
    {
        Assert.IsNotNull(about);

        base.SetUp();
    }

    void Start()
    {
        about.onClick.AddListener(delegate {
            Application.OpenURL(NORDARK_URL);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
