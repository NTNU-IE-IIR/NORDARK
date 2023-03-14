using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class ViewControl : MenuBarItemControl
{
    private const string SHOW_MINIMAP_MESSAGE = "Show minimap";
    private const string HIDE_MINIMAP_MESSAGE = "Hide minimap";
    [SerializeField] private Minimap minimap;
    [SerializeField] private Button minimapButton;
    [SerializeField] private TMP_Text minimapText;
    [SerializeField] private Button fullscreen;
    [SerializeField] private Button windowed;

    void Awake()
    {
        Assert.IsNotNull(minimap);
        Assert.IsNotNull(minimapButton);
        Assert.IsNotNull(minimapText);
        Assert.IsNotNull(fullscreen);
        Assert.IsNotNull(windowed);

        base.SetUp();
    }
    
    void Start()
    {
        minimapButton.onClick.AddListener(() => {
            minimapText.text = minimapText.text == SHOW_MINIMAP_MESSAGE ? HIDE_MINIMAP_MESSAGE : SHOW_MINIMAP_MESSAGE;
            minimap.Show(minimapText.text == HIDE_MINIMAP_MESSAGE);
            gameObject.SetActive(false);
        });

        fullscreen.onClick.AddListener(() =>  {
            Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            gameObject.SetActive(false);
        });
        windowed.onClick.AddListener(() => {
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, false);
            gameObject.SetActive(false);
        });

        minimapText.text = SHOW_MINIMAP_MESSAGE;
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
