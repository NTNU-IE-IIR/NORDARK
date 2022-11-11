using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBarItemControl : MonoBehaviour
{
    private Rect rectangle;
    
    protected void SetUp()
    {
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        rectangle = new Rect(corners[0], corners[2]-corners[0]);
    }

    protected void DeactivateIfCursorOutside()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
            if (!rectangle.Contains(Input.mousePosition)) {
                gameObject.SetActive(false);
            }
        }
    }
}
