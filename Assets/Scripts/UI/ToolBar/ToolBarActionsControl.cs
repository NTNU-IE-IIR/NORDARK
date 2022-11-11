using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ToolBarActionsControl : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private Button save;
    [SerializeField] private Button saveAs;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(save);
        Assert.IsNotNull(saveAs);
    }

    void Start()
    {
        save.onClick.AddListener(sceneManager.Save);
        saveAs.onClick.AddListener(sceneManager.SaveAs);
    }
}
