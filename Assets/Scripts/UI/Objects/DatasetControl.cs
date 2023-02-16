using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class DatasetControl : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private Toggle enable;
    [SerializeField] private Button addVariable;
    [SerializeField] private Button delete;
    private RectTransform rectTransform;

    void Awake()
    {
        Assert.IsNotNull(title);
        Assert.IsNotNull(enable);
        Assert.IsNotNull(addVariable);
        Assert.IsNotNull(delete);
    }

    public void Create(
        string datasetName,
        System.Action<bool> onToggleChanged,
        System.Action onAddVariable,
        System.Action onDelete)
    {
        rectTransform = GetComponent<RectTransform>();

        title.text = datasetName;

        enable.onValueChanged.AddListener(change => onToggleChanged(change));
        addVariable.onClick.AddListener(() => onAddVariable());
        delete.onClick.AddListener(() => onDelete());
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }
}
