using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class FileControl : MenuBarItemControl
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private Button open;
    [SerializeField] private Button save;
    [SerializeField] private Button saveAs;
    [SerializeField] private Button exit;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(open);
        Assert.IsNotNull(save);
        Assert.IsNotNull(saveAs);
        Assert.IsNotNull(exit);

        base.SetUp();
    }
    
    void Start()
    {
        open.onClick.AddListener(delegate {
            sceneManager.Load();
            gameObject.SetActive(false);
        });
        save.onClick.AddListener(delegate {
            sceneManager.Save();
            gameObject.SetActive(false);
        });
        saveAs.onClick.AddListener(delegate {
            sceneManager.SaveAs();
            gameObject.SetActive(false);
        });
        exit.onClick.AddListener(delegate {
            Application.Quit();
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
