using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class VisualizationFeature : MonoBehaviour
{
    [SerializeField] private Material material;
    private const float VISUALIZATION_BLOCK_WIDTH = 5;
    private const float VISUALIZATION_BLOCK_HEIGHT = 5;
    private MeshRenderer meshRenderer;
    private Dictionary<string, object> properties;

    void Awake()
    {
        Assert.IsNotNull(material);

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(material);
    }

    public bool Create(GeoJSON.Net.Feature.Feature feature, MapManager mapManager)
    {
        properties = feature.Properties;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        int trianglesOffset = 0;

        GeoJSON.Net.Geometry.LineString lineString = feature.Geometry as GeoJSON.Net.Geometry.LineString;
        for (int i=0; i<lineString.Coordinates.Count-1; ++i) {
            if (mapManager.IsCoordinateOnMap(new Vector2d(lineString.Coordinates[i])) && mapManager.IsCoordinateOnMap(new Vector2d(lineString.Coordinates[i+1]))) {
                Vector3 currentPosition = mapManager.GetUnityPositionFromCoordinates(new Vector3d(lineString.Coordinates[i]), true);
                Vector3 nextPosition = mapManager.GetUnityPositionFromCoordinates(new Vector3d(lineString.Coordinates[i+1]), true);
                float angle = Mathf.Atan2(nextPosition.z - currentPosition.z, nextPosition.x - currentPosition.x);
                float xShift = VISUALIZATION_BLOCK_WIDTH * Mathf.Sin(angle);
                float zShift = VISUALIZATION_BLOCK_WIDTH * Mathf.Cos(angle);

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
            return false;
        } else {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            gameObject.GetComponent<MeshFilter>().mesh = mesh;
            return true;
        }
    }

    public void SetCurrentIndicator(string indicatorName)
    {
        if (properties.ContainsKey(indicatorName)) {
            float indicator = (float) (double) properties[indicatorName];
            indicator = Mathf.Max(indicator, 0.01f);
            indicator = Mathf.Min(indicator, 0.99f);

            meshRenderer.sharedMaterial.SetTextureOffset("_UnlitColorMap", new Vector2(indicator, 0.5f));
        }
    }
}
