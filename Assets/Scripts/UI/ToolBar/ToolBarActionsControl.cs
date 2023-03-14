using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ToolBarActionsControl : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private MeasureManager measureManager;
    [SerializeField] private Button save;
    [SerializeField] private Button saveAs;
    [SerializeField] private Button measure;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(measureManager);
        Assert.IsNotNull(save);
        Assert.IsNotNull(saveAs);
        Assert.IsNotNull(measure);
    }

    void Start()
    {
        save.onClick.AddListener(sceneManager.Save);
        saveAs.onClick.AddListener(sceneManager.SaveAs);
        measure.onClick.AddListener(measureManager.Measure);
    }
}
