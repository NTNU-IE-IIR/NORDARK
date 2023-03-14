using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class WeightControl : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Slider weight;
    private RectTransform rectTransform;
    
    void Awake()
    {
        Assert.IsNotNull(label);
        Assert.IsNotNull(weight);
    }

    public void Create(string name, System.Action<float> onWeightChanged)
    {
        rectTransform = GetComponent<RectTransform>();
        label.text = name;
        weight.onValueChanged.AddListener(change => onWeightChanged(change));
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }
}
