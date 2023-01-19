using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class DatasetControl : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private Toggle enable;
    [SerializeField] private Button delete;
    private RectTransform rectTransform;

    void Awake()
    {
        Assert.IsNotNull(title);
        Assert.IsNotNull(enable);
        Assert.IsNotNull(delete);
    }

    public void Create(
        string datasetName,
        System.Action<bool> onToggleChanged,
        System.Action onDelete)
    {
        rectTransform = GetComponent<RectTransform>();

        title.text = datasetName;

        enable.onValueChanged.AddListener(change => onToggleChanged(change));
        delete.onClick.AddListener(() => onDelete());
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }
}
