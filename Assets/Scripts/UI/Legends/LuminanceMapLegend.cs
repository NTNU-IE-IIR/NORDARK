using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class LuminanceMapLegend : MonoBehaviour
{
    [SerializeField] private Transform colorsContainer;
    private TMP_Text[] valueLabels;

    void Awake()
    {
        Assert.IsNotNull(colorsContainer);
    }
    
    public Color[] GetColors()
    {
        Color[] colors = new Color[GetNumberOfColors()];
        for (int i = 0; i<colors.Length; ++i) {
            colors[i] = colorsContainer.GetChild(i).GetComponent<Image>().color;
        }
        return colors;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public int GetNumberOfColors()
    {
        return colorsContainer.childCount;
    }

    public void SetValues(float[] values)
    {
        if (valueLabels == null) {
            SetValueLabels();
        }

        if (values.Length == GetNumberOfColors()) {
            for (int i=0; i<values.Length; ++i) {
                valueLabels[i].text = values[i].ToString("0.00");
            }
        }
    }

    private void SetValueLabels()
    {
        valueLabels = new TMP_Text[GetNumberOfColors()];
        for (int i = 0; i<valueLabels.Length; ++i) {
            valueLabels[i] = colorsContainer.GetChild(i).GetChild(0).GetComponent<TMP_Text>();
        }
    }
}
