using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPin : MonoBehaviour
{
    bool isMoving;
    Vector3 baseScale;

    void Awake()
    {
        isMoving = false;
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (isMoving)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 10000, 1 << MapManager.UNITY_LAYER_MAP))
            {
                transform.position = hit.point;
            }
        }
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        if (!active) {
            isMoving = false;
        }
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
        if (moving) {
            gameObject.SetActive(true);
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void MultiplyScale(float scale)
    {
        transform.localScale = baseScale * scale;
    }
}
