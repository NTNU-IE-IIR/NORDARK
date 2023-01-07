using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class GroundTexturesWindow : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TMP_Text description;

    void Awake()
    {
        Assert.IsNotNull(progressBar);
        Assert.IsNotNull(description);
    }

    public void Show(bool show)
    {
        SetDescription("");
        SetProgress(0);
        gameObject.SetActive(show);
    }

    public void SetDescription(string text)
    {
        description.text = text;
    }

    public void SetProgress(float progress)
    {
        progressBar.SetProgress(progress);
    }
}
