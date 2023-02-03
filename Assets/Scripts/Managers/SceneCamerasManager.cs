using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class SceneCamerasManager : MonoBehaviour
{
    [SerializeField] private GameObject sceneCameraPrefab;
    private Camera mainCamera;
    private Rect viewportRect;

    void Awake()
    {
        Assert.IsNotNull(sceneCameraPrefab);

        mainCamera = GetComponent<Camera>();
        viewportRect = mainCamera.rect;
    }

    public void SplitScreen(int numberOfScreens)
    {
        foreach (Transform sceneCamera in transform) {
            Destroy(sceneCamera.gameObject);
        }

        mainCamera.rect = new Rect(
            viewportRect.x,
            numberOfScreens > 1 ? viewportRect.y + viewportRect.height * 0.5f : viewportRect.y,
            viewportRect.width * 1f / ((int) ((numberOfScreens + 1) / 2)),
            numberOfScreens > 1 ? viewportRect.height * 0.5f : viewportRect.height
        );

        for (int i=1; i<numberOfScreens; ++i) {
            SceneCamera sceneCamera = Instantiate(sceneCameraPrefab, transform).GetComponent<SceneCamera>();
            sceneCamera.SetViewport(new Rect(
                i < numberOfScreens/2f ? viewportRect.x + mainCamera.rect.width * i : viewportRect.x + mainCamera.rect.width * i - viewportRect.width,
                i < numberOfScreens/2f ? viewportRect.y + viewportRect.height * 0.5f : viewportRect.y,
                i < numberOfScreens/2f ? mainCamera.rect.width : viewportRect.width / (numberOfScreens - i),
                mainCamera.rect.height
            ));
        }
    }
}