using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class SceneCamerasManager : MonoBehaviour
{
    public const int HIDDEN_FROM_CAMERA_LAYER = 12;
    [SerializeField] private LightsManager lightsManager;
    [SerializeField] private GameObject sceneCameraPrefab;
    private Camera mainCamera;
    private Rect viewportRect;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);
        Assert.IsNotNull(sceneCameraPrefab);

        mainCamera = GetComponent<Camera>();
        viewportRect = mainCamera.rect;
    }

    public void DisplayLuminanceMaps(int numberOfScreens, int maxNumberOfScreens)
    {
        for (int i=1; i<maxNumberOfScreens; ++i) {
            lightsManager.DeleteAllLightsFromConfiguration(i);
        }

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
            sceneCamera.Create(new Rect(
                i < numberOfScreens/2f ? viewportRect.x + mainCamera.rect.width * i : viewportRect.x + mainCamera.rect.width * i - viewportRect.width,
                i < numberOfScreens/2f ? viewportRect.y + viewportRect.height * 0.5f : viewportRect.y,
                i < numberOfScreens/2f ? mainCamera.rect.width : viewportRect.width / (numberOfScreens - i),
                mainCamera.rect.height
            ), lightsManager, i);
        }
    }

    public void DisplayLineVisualization()
    {
        
    }

    public void SetConfiguration(string path, int configurationIndex)
    {
        lightsManager.DeleteAllLightsFromConfiguration(configurationIndex);

        GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(path);

        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            if (feature.Properties.ContainsKey("type")) {
                if (string.Equals(feature.Properties["type"] as string, "light")) {
                    lightsManager.Create(feature, configurationIndex);
                }
            }
        }
    }
}