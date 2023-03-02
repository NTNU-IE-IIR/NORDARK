using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class HeatmapControl : MonoBehaviour
{
    [SerializeField] private ComputationRectangle computationRectangle;
    [SerializeField] private GameObject figure;
    [SerializeField] private Heatmap heatmap;
    [SerializeField] private TMP_Text status;
    [SerializeField] private GameObject legend;
    [SerializeField] private TMP_Text minLegend;
    [SerializeField] private TMP_Text maxLegend;
    [SerializeField] private Transform labelsX;
    [SerializeField] private Transform labelsY;
    [SerializeField] private Button refreshButton;

    void Awake()
    {
        Assert.IsNotNull(computationRectangle);
        Assert.IsNotNull(figure);
        Assert.IsNotNull(heatmap);
        Assert.IsNotNull(status);
        Assert.IsNotNull(legend);
        Assert.IsNotNull(minLegend);
        Assert.IsNotNull(maxLegend);
        Assert.IsNotNull(labelsX);
        Assert.IsNotNull(labelsY);
        Assert.IsNotNull(refreshButton);
    }

    public void Show(bool display)
    {
        gameObject.SetActive(display);
    }

    public void SetWaitingForDefinition()
    {
        SetStatus("Waiting for rectange definition...");
    }

    public void SetComputing()
    {
        SetStatus("Computing...");
    }

    public void DisplayValues(bool display)
    {
        heatmap.DisplayValues(display);
    }

    public void SetHeatmap(
        Vector3[] positions,
        float[] values,
        System.Action onRefresh,
        float yMinimum = IComputationObject.EXTREMUM_DEFAULT_VALUE,
        float yMaximum = IComputationObject.EXTREMUM_DEFAULT_VALUE)
    {
        figure.SetActive(true);

        if (yMinimum > yMaximum && yMinimum != IComputationObject.EXTREMUM_DEFAULT_VALUE) {
            (yMinimum, yMaximum) = (yMaximum, yMinimum);
        }
        if (yMinimum == yMaximum && yMinimum != IComputationObject.EXTREMUM_DEFAULT_VALUE) {
            yMaximum += 0.001f;
        }

        float minValue = yMinimum == IComputationObject.EXTREMUM_DEFAULT_VALUE ? values.Min() : yMinimum;
        float maxValue = yMaximum == IComputationObject.EXTREMUM_DEFAULT_VALUE ? values.Max() : yMaximum;
        heatmap.SetValues(values, minValue, maxValue);

        float step = 1f / (labelsX.childCount-1);
        float t = 0;
        foreach (Transform labelX in labelsX) {
            labelX.GetComponent<TMP_Text>().text = Mathf.Abs(positions.First().x - Vector3.Lerp(positions.First(), positions.Last(), t).x).ToString("0.0");
            t += step;
        }

        t = 0;
        foreach (Transform labelY in labelsY) {
            labelY.GetComponent<TMP_Text>().text = Mathf.Abs(positions.First().z - Vector3.Lerp(positions.First(), positions.Last(), t).z).ToString("0.0");
            t += step;
        }

        minLegend.text = minValue.ToString("0.00") + " cd/m²";
        maxLegend.text = maxValue.ToString("0.00") + " cd/m²";

        refreshButton.onClick.RemoveAllListeners();
        refreshButton.onClick.AddListener(() => {
            onRefresh();
        });
    }

    public void OnMouseOver(Vector2 normalizedPosition)
    {
        computationRectangle.OnMouseOver(normalizedPosition);
    }

    public void OnMouseExit()
    {
        computationRectangle.OnMouseExit();
    }

    private void SetStatus(string message)
    {
        figure.SetActive(false);
        status.text = message;
    }
}