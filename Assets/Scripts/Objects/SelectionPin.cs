using UnityEngine;
using UnityEngine.Assertions;

public class SelectionPin : MonoBehaviour
{
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    private bool isMoving;

    void Awake()
    {
        Assert.IsNotNull(sceneCamerasManager);

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

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
