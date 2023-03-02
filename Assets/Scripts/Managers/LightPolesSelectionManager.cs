using UnityEngine;
using UnityEngine.Assertions;

public class LightPolesSelectionManager : MonoBehaviour
{
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private LightsTabControl lightsTabControl;
    private bool isDrawing;
    private bool isDragging;
    private Vector3 dragStartPosition;
    private Texture2D whiteTexture;

    void Awake()
    {
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(lightsTabControl);

        isDrawing = false;
        isDragging = false;

        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    void Update()
    {
        if (isDrawing) {
            if (Input.GetMouseButtonDown(0)) {
                isDragging = true;
                dragStartPosition = Input.mousePosition;
            }

            if (isDragging && Input.GetMouseButtonUp(0)) {
                Stop();
                lightPolesManager.ClearSelectedLightPoles();
                
                if (sceneCamerasManager.isView2D()) {
                    lightPolesManager.SelectLightPolesWithinPositions(
                        sceneCamerasManager.ScreenToWorldPoint(dragStartPosition),
                        sceneCamerasManager.ScreenToWorldPoint(Input.mousePosition)
                    );
                } else {
                    SelectLightPolesWithRays(dragStartPosition, Input.mousePosition);
                }
            }
        }
    }

    void OnGUI()
    {
        if (isDrawing && isDragging) {
            var rect = GetScreenRect(dragStartPosition, Input.mousePosition);
            DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }

    void OnDestroy()
    {
        Destroy(whiteTexture);
    }

    public void StartDrawing()
    {
        if (lightsTabControl.IsActive()) {
            isDrawing = true;
            isDragging = false;
        }
    }

    public void Stop()
    {
        isDrawing = false;
        isDragging = false;
    }

    private void SelectLightPolesWithRays(Vector3 vertexA, Vector3 vertexB)
    {
        int xMin = (int) (vertexA.x < vertexB.x ? vertexA.x : vertexB.x);
        int xMax = (int) (vertexA.x < vertexB.x ? vertexB.x : vertexA.x);
        int yMin = (int) (vertexA.y < vertexB.y ? vertexA.y : vertexB.y);
        int yMax = (int) (vertexA.y < vertexB.y ? vertexB.y : vertexA.y);
        
        RaycastHit hitInfo = new RaycastHit();
        for (int x=xMin; x<xMax; ++x) {
            for (int y=yMin; y<yMax; ++y) {
                if (Physics.Raycast(sceneCamerasManager.ScreenPointToRay(new Vector3(x, y, 0)), out hitInfo)) {
                    lightPolesManager.AddLightPrefabToSelectedLightPoles(
                        hitInfo.transform.gameObject.GetComponent<LightPrefab>(),
                        true
                    );
                }
            }
        }
    }

    private Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;

        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    private void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
}
