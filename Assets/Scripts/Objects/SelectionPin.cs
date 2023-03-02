using UnityEngine;

public class SelectionPin : MonoBehaviour
{
    private SceneCamerasManager sceneCamerasManager;
    private bool isMoving;

    void Awake()
    {
        isMoving = false;
    }

    void Update()
    {
        if (isMoving) {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            Ray ray = sceneCamerasManager.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (!isOverUI && Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                transform.position = hit.point;
            }
        }
    }

    public void Create(SceneCamerasManager sceneCamerasManager)
    {
        this.sceneCamerasManager = sceneCamerasManager;
    }

    public void SetMoving(bool moving)
    {
        isMoving = moving;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
