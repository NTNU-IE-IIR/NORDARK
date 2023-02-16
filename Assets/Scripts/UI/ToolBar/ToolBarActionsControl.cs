using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class ToolBarActionsControl : MonoBehaviour
{
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private MeasureManager measureManager;
    [SerializeField] private Button save;
    [SerializeField] private Button saveAs;
    [SerializeField] private Button view;
    [SerializeField] private Button measure;
    [SerializeField] private Sprite view2DSprite;
    [SerializeField] private Sprite view3DSprite;
    private Image viewImage;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(measureManager);
        Assert.IsNotNull(save);
        Assert.IsNotNull(saveAs);
        Assert.IsNotNull(view);
        Assert.IsNotNull(measure);
        Assert.IsNotNull(view2DSprite);
        Assert.IsNotNull(view3DSprite);

        viewImage = view.GetComponent<Image>();
    }

    void Start()
    {
        save.onClick.AddListener(sceneManager.Save);
        saveAs.onClick.AddListener(sceneManager.SaveAs);
        view.onClick.AddListener(ChangeViewType);
        measure.onClick.AddListener(measureManager.Measure);
    }

    private void ChangeViewType()
    {
        if (viewImage.sprite == view2DSprite) {
            viewImage.sprite = view3DSprite;
        } else {
            viewImage.sprite = view2DSprite;
        }
        sceneCamerasManager.ChangeViewType();
    }
}
