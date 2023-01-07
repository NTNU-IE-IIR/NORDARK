using UnityEngine;
using UnityEngine.Assertions;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform barTransform;
    private float barSize;

    void Awake()
    {
        Assert.IsNotNull(barTransform);

        barSize = GetComponent<RectTransform>().sizeDelta.x;
    }

    public void SetProgress(float progress)
    {
        barTransform.sizeDelta = new Vector2(progress * barSize, barTransform.sizeDelta.y);
    }
}
