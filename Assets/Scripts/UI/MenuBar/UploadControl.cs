using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class UploadControl : MenuBarItemControl
{
    [SerializeField] private IESManager iesManager;
    [SerializeField] private Button iesFile;

    void Awake()
    {
        Assert.IsNotNull(iesManager);
        Assert.IsNotNull(iesFile);

        base.SetUp();
    }
    
    void Start()
    {
        iesFile.onClick.AddListener(delegate {
            iesManager.Upload();
            gameObject.SetActive(false);
        });
    }

    void Update()
    {
        base.DeactivateIfCursorOutside();
    }
}
