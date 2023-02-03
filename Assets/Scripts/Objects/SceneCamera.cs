using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
    [SerializeField] private Light lightTest;
    private Camera sceneCamera;

    void Awake()
    {
        sceneCamera = GetComponent<Camera>();
    }

    private void OnPreCull()
    {
        if (lightTest != null) {
            lightTest.enabled = false;
        }
    }
    private void OnPreRender()
    {
        if (lightTest != null) {
            lightTest.enabled = false;
        }
    }
    private void OnPostRender()
    {
        if (lightTest != null) {
            lightTest.enabled = true;
        }
    }

    public void SetViewport(Rect rect)
    {
        sceneCamera.rect = rect;
    }
}