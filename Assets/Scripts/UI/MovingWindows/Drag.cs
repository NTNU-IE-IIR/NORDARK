using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    void Awake()
    {
        Assert.IsNotNull(canvas);
    }

    public void DragHandler(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData) data;
        
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform) canvas.transform,
            pointerData.position,
            canvas.worldCamera,
            out position
        );

        transform.position = canvas.transform.TransformPoint(position);
    }
}
