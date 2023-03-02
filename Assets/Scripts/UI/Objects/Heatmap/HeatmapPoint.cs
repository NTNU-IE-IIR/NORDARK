using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class HeatmapPoint : MonoBehaviour
{
    [SerializeField] private TMP_Text value;
    [SerializeField] private TMP_Text cross;
    public enum TextPositionVertical
    {
        Top,
        Bottom
    }
    public enum TextPositionHorizontal
    {
        Left,
        Middle,
        Right
    }
    private float xPositionAbs;
    private float yPositionAbs;

    void Awake()
    {
        Assert.IsNotNull(value);
        Assert.IsNotNull(cross);

        xPositionAbs = Mathf.Abs(value.transform.localPosition.x);
        yPositionAbs = Mathf.Abs(value.transform.localPosition.y);
    }

    public void Create(float value, TextPositionHorizontal textPositionHorizontal, TextPositionVertical textPositionVertical, float scaleFactor)
    {
        this.value.text = value.ToString("0.00");

        float xPosition;
        if (textPositionHorizontal == TextPositionHorizontal.Left) {
            xPosition = -xPositionAbs;
            this.value.horizontalAlignment = HorizontalAlignmentOptions.Right;
        } else if (textPositionHorizontal == TextPositionHorizontal.Right) {
            xPosition = xPositionAbs;
            this.value.horizontalAlignment = HorizontalAlignmentOptions.Left;
        } else {
            xPosition = 0;
            this.value.horizontalAlignment = HorizontalAlignmentOptions.Center;
        }

        float yPosition;
        if (textPositionVertical == TextPositionVertical.Top) {
            yPosition = yPositionAbs;
            this.value.verticalAlignment = VerticalAlignmentOptions.Bottom;
        } else {
            yPosition = -yPositionAbs;
            this.value.verticalAlignment = VerticalAlignmentOptions.Top;
        }

        this.value.transform.localPosition = new Vector3(
            xPosition,
            yPosition,
            this.value.transform.localPosition.z
        );

        this.value.fontSize *= scaleFactor;
        this.cross.fontSize *= scaleFactor;
    }
}
