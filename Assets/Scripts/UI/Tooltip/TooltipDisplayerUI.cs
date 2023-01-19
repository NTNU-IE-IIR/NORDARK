using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipDisplayerUI : MonoBehaviour
{
    private string text;

    void Awake()
    {
        text = "";
        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
        entryPointerEnter.eventID = EventTriggerType.PointerEnter;
        entryPointerEnter.callback.AddListener((data) => {
            TooltipControl.DisplayTooltip(true, text);
        });
        eventTrigger.triggers.Add(entryPointerEnter);

        EventTrigger.Entry entryPointerExit = new EventTrigger.Entry();
        entryPointerExit.eventID = EventTriggerType.PointerExit;
        entryPointerExit.callback.AddListener((data) => {
            TooltipControl.DisplayTooltip(false);
        });
        eventTrigger.triggers.Add(entryPointerExit);
    }

    public void SetText(string text)
    {
        this.text = text;
    }
}