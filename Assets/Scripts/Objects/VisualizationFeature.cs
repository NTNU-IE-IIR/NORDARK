using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(TooltipDisplayer), typeof(MeshRenderer))]
public class VisualizationFeature : MonoBehaviour
{
    private enum Shape
    {
        Line,
        Bar
    }
    private const float LINE_VISUALIZATION_WIDTH = 5;
    private const float LINE_VISUALIZATION_HEIGHT = 5;
    private const float BAR_VISUALIZATION_WIDTH = 5;
    private const float BAR_VISUALIZATION_HEIGHT = 100;
    [SerializeField] private Material material;
    private TooltipDisplayer tooltipDisplayer;
    private MeshRenderer meshRenderer;
    private Dictionary<string, float> variableValues;
    private string datasetName;
    private bool isCreated;
    private Shape shape;

    void Awake()
    {
        Assert.IsNotNull(material);

        tooltipDisplayer = GetComponent<TooltipDisplayer>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(material);
        variableValues = new Dictionary<string, float>();
        isCreated = false;
    }

    public void Create(string datasetName, Dictionary<string, float> weights, GeoJSON.Net.Feature.Feature feature, MapManager mapManager)
    {
        this.datasetName = datasetName;

        foreach (string variable in feature.Properties.Keys) {
            try {
                // A float property must first be converted to double, if not an error occurs
                variableValues[variable] = (float) (double) feature.Properties[variable];
            } catch (System.Exception) {}
        }

        Mesh mesh = null;
        switch (feature.Geometry)
        {
            case GeoJSON.Net.Geometry.Point point:
                mesh = CreateBarMesh(new Coordinate(point.Coordinates), mapManager);
                break;
            case GeoJSON.Net.Geometry.MultiPoint multiPoint:
                if (multiPoint.Coordinates.Count > 0) {
                    mesh = CreateBarMesh(new Coordinate(multiPoint.Coordinates[0].Coordinates), mapManager);
                }
                break;
            case GeoJSON.Net.Geometry.LineString lineString:
                mesh = CreateLineMesh(lineString.Coordinates, mapManager);
                break;
            case GeoJSON.Net.Geometry.MultiLineString multiLineString:
                if (multiLineString.Coordinates.Count > 0) {
                    mesh = CreateLineMesh(multiLineString.Coordinates[0].Coordinates, mapManager);
                }
                break;
            case GeoJSON.Net.Geometry.Polygon polygon:
                mesh = CreateBarMeshFromPolygon(polygon, mapManager);
                break;
            case GeoJSON.Net.Geometry.MultiPolygon multiPolygon:
                if (multiPolygon.Coordinates.Count > 0) {
                    mesh = CreateBarMeshFromPolygon(multiPolygon.Coordinates[0], mapManager);
                }
                break;
            default:
                break;
        }

        isCreated = mesh != null && mesh.vertexCount > 0;
        if (isCreated) {
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            SetWeights(weights);
        } else {
            Destroy(gameObject);
        }

    }

    public bool IsCreated()
    {
        return isCreated;
    }

    public void SetWeights(Dictionary<string, float> weights)
    {
        float weightSum = weights.Keys.Sum(variable => variableValues.ContainsKey(variable) ? weights[variable] : 0);

        Dictionary<string, float> weightsNormalized = new Dictionary<string, float>();
        foreach (string variable in weights.Keys) {
            if (variableValues.ContainsKey(variable)) {
                weightsNormalized[variable] = weights[variable] / weightSum;
            }
        }

        float value = 0;
        foreach (string variable in weightsNormalized.Keys) {
            value += weightsNormalized[variable] * variableValues[variable];
        }
        
        tooltipDisplayer.SetText(datasetName + "\n" + value.ToString());

        // Colors on the edge (when value < 0.01 or value > 0.99) are not good
        value = Mathf.Max(value, 0.01f);
        value = Mathf.Min(value, 0.99f);

        meshRenderer.sharedMaterial.SetTextureOffset("_UnlitColorMap", new Vector2(value, 0.5f));

        if (shape == Shape.Bar) {
            transform.localScale = new Vector3(
                transform.localScale.x,
                value,
                transform.localScale.z
            );
        }
    }

    private Mesh CreateBarMeshFromPolygon(GeoJSON.Net.Geometry.Polygon polygon, MapManager mapManager)
    {
        // See https://www.rfc-editor.org/rfc/rfc7946#section-3.1.6:
        // A polygon is an array of "linear rings", the first one being
        // the exterior ring of the polygon
        if (polygon.Coordinates.Count > 0) {
            Coordinate centroid = new Coordinate();
            foreach (GeoJSON.Net.Geometry.IPosition point in polygon.Coordinates[0].Coordinates) {
                centroid += new Coordinate(point);
            }
            centroid /= polygon.Coordinates[0].Coordinates.Count;
            return CreateBarMesh(centroid, mapManager);
        } else {
            return null;
        }
    }

    private Mesh CreateLineMesh(ReadOnlyCollection<GeoJSON.Net.Geometry.IPosition> coordinates, MapManager mapManager)
    {
        shape = Shape.Line;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        int trianglesOffset = 0;
        Vector3 lastShift = new Vector3();

        for (int i=0; i<coordinates.Count-1; ++i) {
            Coordinate currentCoordinate = new Coordinate(coordinates[i]);
            Coordinate nextCoordinate = new Coordinate(coordinates[i+1]);

            if (mapManager.IsCoordinateOnMap(currentCoordinate) && mapManager.IsCoordinateOnMap(nextCoordinate)) {
                Vector3 currentPosition = mapManager.GetUnityPositionFromCoordinates(currentCoordinate, true);
                Vector3 nextPosition = mapManager.GetUnityPositionFromCoordinates(nextCoordinate, true);

                float angle = Mathf.Atan2(nextPosition.z - currentPosition.z, nextPosition.x - currentPosition.x);
                float xShift = LINE_VISUALIZATION_WIDTH * Mathf.Sin(angle);
                float zShift = LINE_VISUALIZATION_WIDTH * Mathf.Cos(angle);

                if (i > 0) {
                    vertices.AddRange(new List<Vector3> {
                        currentPosition + lastShift,
                        nextPosition + new Vector3(xShift, 0, -zShift),
                        nextPosition + new Vector3(-xShift, 0, zShift),
                        currentPosition - lastShift,

                        currentPosition + lastShift + new Vector3(0, LINE_VISUALIZATION_HEIGHT, 0),
                        nextPosition + new Vector3(xShift, LINE_VISUALIZATION_HEIGHT, -zShift),
                        nextPosition + new Vector3(-xShift, LINE_VISUALIZATION_HEIGHT, zShift),
                        currentPosition - lastShift + new Vector3(0, LINE_VISUALIZATION_HEIGHT, 0),
                    });
                } else {
                    vertices.AddRange(new List<Vector3> {
                        currentPosition + new Vector3(xShift, 0, -zShift),
                        nextPosition + new Vector3(xShift, 0, -zShift),
                        nextPosition + new Vector3(-xShift, 0, zShift),
                        currentPosition + new Vector3(-xShift, 0, zShift),

                        currentPosition + new Vector3(xShift, LINE_VISUALIZATION_HEIGHT, -zShift),
                        nextPosition + new Vector3(xShift, LINE_VISUALIZATION_HEIGHT, -zShift),
                        nextPosition + new Vector3(-xShift, LINE_VISUALIZATION_HEIGHT, zShift),
                        currentPosition + new Vector3(-xShift, LINE_VISUALIZATION_HEIGHT, zShift),
                    });
                }
                lastShift = new Vector3(xShift, 0, -zShift);

                uv.AddRange(CreateCubeUVs());
                triangles.AddRange(CreateCubeTriangles(trianglesOffset));
                trianglesOffset += 8;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

    private Mesh CreateBarMesh(Coordinate coordinate, MapManager mapManager)
    {
        shape = Shape.Bar;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();

        if (mapManager.IsCoordinateOnMap(coordinate)) {
            transform.position = mapManager.GetUnityPositionFromCoordinates(coordinate, true);

            vertices.AddRange(new List<Vector3> {
                new Vector3(-BAR_VISUALIZATION_WIDTH/2, 0, -BAR_VISUALIZATION_WIDTH/2),
                new Vector3(BAR_VISUALIZATION_WIDTH/2, 0, -BAR_VISUALIZATION_WIDTH/2),
                new Vector3(BAR_VISUALIZATION_WIDTH/2, 0, BAR_VISUALIZATION_WIDTH/2),
                new Vector3(-BAR_VISUALIZATION_WIDTH/2, 0, BAR_VISUALIZATION_WIDTH/2),

                new Vector3(-BAR_VISUALIZATION_WIDTH/2, BAR_VISUALIZATION_HEIGHT, -BAR_VISUALIZATION_WIDTH/2),
                new Vector3(BAR_VISUALIZATION_WIDTH/2, BAR_VISUALIZATION_HEIGHT, -BAR_VISUALIZATION_WIDTH/2),
                new Vector3(BAR_VISUALIZATION_WIDTH/2, BAR_VISUALIZATION_HEIGHT, BAR_VISUALIZATION_WIDTH/2),
                 new Vector3(-BAR_VISUALIZATION_WIDTH/2, BAR_VISUALIZATION_HEIGHT, BAR_VISUALIZATION_WIDTH/2)
            });

            uv.AddRange(CreateCubeUVs());
            triangles.AddRange(CreateCubeTriangles(0));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();

        return mesh;
    }

    private List<Vector2> CreateCubeUVs()
    {
        return new List<Vector2> {
            new Vector2 (0, 0),
            new Vector2 (1, 0),
            new Vector2 (1, 1),
            new Vector2 (0, 1),
            
            new Vector2 (0, 0),
            new Vector2 (1, 0),
            new Vector2 (1, 1),
            new Vector2 (0, 1),
        };
    }

    private List<int> CreateCubeTriangles(int offset)
    {
        return new List<int> {
            // bottom
            offset + 0, offset + 1, offset + 3,
            offset + 3, offset + 1, offset + 2,
            
            // top
            offset + 4, offset + 7, offset + 5,
            offset + 7, offset + 6, offset + 5,
            
            // left
            offset + 4, offset + 5, offset + 1,
            offset + 4, offset + 1, offset + 0,
            
            // right
            offset + 6, offset + 7, offset + 2,
            offset + 7, offset + 3, offset + 2,
            
            // front
            offset + 7, offset + 4, offset + 0,
            offset + 7, offset + 0, offset + 3,
            
            // back
            offset + 5, offset + 6, offset + 2,
            offset + 5, offset + 2, offset + 1
        };
    }
}
