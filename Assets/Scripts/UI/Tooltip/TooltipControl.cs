using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using TMPro;

public class TooltipControl : MonoBehaviour
{
    private const float TEXT_PADDING_SIZE = 4;
    private const int UI_LAYER = 5;
    [SerializeField] private TMP_Text text;
    private RectTransform rectTransform;
    private static TooltipControl instance;

    void Awake()
    {
        Assert.IsNotNull(text);

        instance = this;
        rectTransform = GetComponent<RectTransform>();
        DisplayTooltip(false, "");
    }

    void Update()
    {
        transform.position = Input.mousePosition + new Vector3(rectTransform.sizeDelta.x/2, rectTransform.sizeDelta.y/2, 0);
        HideIfOverUI();
    }

    public static void DisplayTooltip(bool display, string message = "")
    {
        instance.Display(display, message);
    }

    private void Display(bool display, string message)
    {
        gameObject.SetActive(display);
        text.text = message;
        text.ForceMeshUpdate();

        rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight) + new Vector2(TEXT_PADDING_SIZE*2, TEXT_PADDING_SIZE*2);
        transform.position = Input.mousePosition + new Vector3(rectTransform.sizeDelta.x/2, rectTransform.sizeDelta.y/2, 0);
    }

    private void HideIfOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);

        foreach (RaycastResult raysastResult in raysastResults) {
            if (raysastResult.gameObject.layer == UI_LAYER) {
                DisplayTooltip(false);
            }
        }
    }
}
