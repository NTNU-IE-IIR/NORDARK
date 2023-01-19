using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class Caption : MonoBehaviour
{
    [SerializeField] private Image colorImage;
    [SerializeField] private TMP_Text label;

    void Awake()
    {
        Assert.IsNotNull(colorImage);
        Assert.IsNotNull(label);
    }

    public void Create(Color color, string text)
    {
        colorImage.color = color;
        label.text = text;
    }
}