using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VisualizationFeature : MonoBehaviour
{
    private const float VISUALIZATION_BLOCK_WIDTH = 5;
    private const float VISUALIZATION_BLOCK_HEIGHT = 5;
    [SerializeField] private Material material;
    private MeshRenderer meshRenderer;
    private Dictionary<string, object> properties;
    private string currentIndicator;
    private bool isCreated;

    void Awake()
    {
        Assert.IsNotNull(material);

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(material);
        currentIndicator = "";
        isCreated = false;
    }

    public void Create(GeoJSON.Net.Feature.Feature feature, MapManager mapManager)
    {
        properties = feature.Properties;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        int trianglesOffset = 0;
        Vector3 lastShift = new Vector3();

        GeoJSON.Net.Geometry.LineString lineString = feature.Geometry as GeoJSON.Net.Geometry.LineString;
        for (int i=0; i<lineString.Coordinates.Count-1; ++i) {
            Vector3d currentCoordinate = new Vector3d(lineString.Coordinates[i]);
            Vector3d nextCoordinate = new Vector3d(lineString.Coordinates[i+1]);

            if (mapManager.IsCoordinateOnMap(currentCoordinate) && mapManager.IsCoordinateOnMap(nextCoordinate)) {
                Vector3 currentPosition = mapManager.GetUnityPositionFromCoordinates(currentCoordinate, true);
                Vector3 nextPosition = mapManager.GetUnityPositionFromCoordinates(nextCoordinate, true);

                float angle = Mathf.Atan2(nextPosition.z - currentPosition.z, nextPosition.x - currentPosition.x);
                float xShift = VISUALIZATION_BLOCK_WIDTH * Mathf.Sin(angle);
                float zShift = VISUALIZATION_BLOCK_WIDTH * Mathf.Cos(angle);

                if (i > 0) {
                    vertices.AddRange(new List<Vector3> {
                        currentPosition + lastShift,
                        nextPosition + new Vector3(xShift, 0, -zShift),
                        nextPosition + new Vector3(-xShift, 0, zShift),
                        currentPosition - lastShift,

                        currentPosition + lastShift + new Vector3(0, VISUALIZATION_BLOCK_HEIGHT, 0),
                        nextPosition + new Vector3(xShift, VISUALIZATION_BLOCK_HEIGHT, -zShift),
                        nextPosition + new Vector3(-xShift, VISUALIZATION_BLOCK_HEIGHT, zShift),
                        currentPosition - lastShift + new Vector3(0, VISUALIZATION_BLOCK_HEIGHT, 0),
                    });
                } else {
                    vertices.AddRange(new List<Vector3> {
                        currentPosition + new Vector3(xShift, 0, -zShift),
                        nextPosition + new Vector3(xShift, 0, -zShift),
                        nextPosition + new Vector3(-xShift, 0, zShift),
                        currentPosition + new Vector3(-xShift, 0, zShift),

                        currentPosition + new Vector3(xShift, VISUALIZATION_BLOCK_HEIGHT, -zShift),
                        nextPosition + new Vector3(xShift, VISUALIZATION_BLOCK_HEIGHT, -zShift),
                        nextPosition + new Vector3(-xShift, VISUALIZATION_BLOCK_HEIGHT, zShift),
                        currentPosition + new Vector3(-xShift, VISUALIZATION_BLOCK_HEIGHT, zShift),
                    });
                }

                lastShift = new Vector3(xShift, 0, -zShift);

                uv.AddRange(new List<Vector2> {
                    new Vector2 (0, 0),
                    new Vector2 (1, 0),
                    new Vector2 (1, 1),
                    new Vector2 (0, 1),
                    
                    new Vector2 (0, 0),
                    new Vector2 (1, 0),
                    new Vector2 (1, 1),
                    new Vector2 (0, 1),
                });

                triangles.AddRange(new List<int> {
                    // bottom
                    trianglesOffset + 0, trianglesOffset + 1, trianglesOffset + 3,
                    trianglesOffset + 3, trianglesOffset + 1, trianglesOffset + 2,
                    
                    // top
                    trianglesOffset + 4, trianglesOffset + 7, trianglesOffset + 5,
                    trianglesOffset + 7, trianglesOffset + 6, trianglesOffset + 5,
                    
                    // left
                    trianglesOffset + 4, trianglesOffset + 5, trianglesOffset + 1,
                    trianglesOffset + 4, trianglesOffset + 1, trianglesOffset + 0,
                    
                    // right
                    trianglesOffset + 6, trianglesOffset + 7, trianglesOffset + 2,
                    trianglesOffset + 7, trianglesOffset + 3, trianglesOffset + 2,
                    
                    // front
                    trianglesOffset + 7, trianglesOffset + 4, trianglesOffset + 0,
                    trianglesOffset + 7, trianglesOffset + 0, trianglesOffset + 3,
                    
                    // back
                    trianglesOffset + 5, trianglesOffset + 6, trianglesOffset + 2,
                    trianglesOffset + 5, trianglesOffset + 2, trianglesOffset + 1
                });
                trianglesOffset += 8;
            }
        }

        if (vertices.Count < 1) {
            Destroy(gameObject);
        } else {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        isCreated = vertices.Count > 0;
    }

    public void SetCurrentIndicator(string indicatorName)
    {
        if (properties.ContainsKey(indicatorName)) {
            float indicator = (float) (double) properties[indicatorName];
            currentIndicator = indicatorName + "\n" + indicator.ToString();

            indicator = Mathf.Max(indicator, 0.01f);
            indicator = Mathf.Min(indicator, 0.99f);

            meshRenderer.sharedMaterial.SetTextureOffset("_UnlitColorMap", new Vector2(indicator, 0.5f));
        } else {
            currentIndicator = indicatorName + "\nNo value";
            meshRenderer.sharedMaterial.SetTextureOffset("_UnlitColorMap", new Vector2(0, 0.5f));
        }
    }

    public bool IsCreated()
    {
        return isCreated;
    }

    void OnMouseEnter()
    {
        TooltipControl.DisplayTooltip(true, currentIndicator);
    }

    void OnMouseExit()
    {
        TooltipControl.DisplayTooltip(false);
    }
}
