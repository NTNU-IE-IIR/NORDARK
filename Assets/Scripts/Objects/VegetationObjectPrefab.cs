using UnityEngine;

public class VegetationObjectPrefab: MonoBehaviour
{
    private bool isMoving = false;
    private SceneCamerasManager sceneCamerasManager;

    void Update()
    {
        if (isMoving) {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            Ray ray = sceneCamerasManager.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (!isOverUI && Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << TerrainManager.TERRAIN_LAYER)) {
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

    public void Rotate(float rotation)
    {
        Vector3 angles = transform.eulerAngles;
        angles.y = rotation;
        transform.eulerAngles = angles;
    }

    public void Show(bool show) {
        gameObject.SetActive(show);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
