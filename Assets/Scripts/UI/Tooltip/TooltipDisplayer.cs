using UnityEngine;

public class TooltipDisplayer : MonoBehaviour
{
    private string text;

    void Awake()
    {
        text = "";
    }

    void OnMouseEnter()
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