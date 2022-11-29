using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ViewControl : MenuBarItemControl
{
    [SerializeField] private Button fullscreen;
    [SerializeField] private Button windowed;

    void Awake()
    {
        Assert.IsNotNull(fullscreen);
        Assert.IsNotNull(windowed);

        base.SetUp();
    }
    
    void Start()
    {
        fullscreen.onClick.AddListener(delegate {
            Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            gameObject.SetActive(false);
        });
        windowed.onClick.AddListener(delegate {
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, false);
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
