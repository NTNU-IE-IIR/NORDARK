using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class DatasetControl : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Dropdown indicators;
    [SerializeField] private Toggle enable;
    [SerializeField] private Button delete;
    private RectTransform rectTransform;

    void Awake()
    {
        Assert.IsNotNull(title);
        Assert.IsNotNull(indicators);
        Assert.IsNotNull(enable);
        Assert.IsNotNull(delete);

        rectTransform = GetComponent<RectTransform>();
    }

    public void Create(
        string datasetName,
        List<string> indicatorNames,
        System.Action<bool> onToggleChanged,
        System.Action<string> onIndicatorChanged,
        System.Action onDelete)
    {
        title.text = datasetName;
        indicators.AddOptions(indicatorNames);

        enable.onValueChanged.AddListener(change => onToggleChanged(change));
        indicators.onValueChanged.AddListener(change => onIndicatorChanged(indicators.options[change].text));
        delete.onClick.AddListener(() => onDelete());
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }
}
