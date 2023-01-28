using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class HeatmapControl : MonoBehaviour
{
    [SerializeField] private Heatmap heatmap;
    [SerializeField] private TMP_Text status;

    void Awake()
    {
        Assert.IsNotNull(status);
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

    public void SetHeatmap(float[] values)
    {
        heatmap.SetValues(values);
        heatmap.gameObject.SetActive(true);
    }

    private void SetStatus(string message)
    {
        status.text = message;
        heatmap.gameObject.SetActive(false);
    }
}