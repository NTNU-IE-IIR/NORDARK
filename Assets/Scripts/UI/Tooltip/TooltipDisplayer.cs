using UnityEngine;

// To be added on any non UI component that should display a tooltip when hovered
public class TooltipDisplayer : MonoBehaviour
{
    private string text;

    void Awake()
    {
        text = "";
    }

    void OnMouseOver()
    {
        TooltipControl.DisplayTooltip(true, text);
    }

    void OnMouseExit()
    {
        TooltipControl.DisplayTooltip(false);
    }

    public void SetText(string text)
    {
        this.text = text;
    }
}