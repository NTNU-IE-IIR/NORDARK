using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(TooltipDisplayer))]
public class ComputationLine : MonoBehaviour, IComputationObject
{
    private const float LINE_HEIGHT = 1;
    private const int LINE_LAYER = 11;
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private ObjectVisualizationControl lineVisualizationControl;
    [SerializeField] private GraphControl graphControl;
    private LineRenderer line;
    private MeshCollider meshCollider;
    private TooltipDisplayer tooltipDisplayer;
    private bool isCreatingLine;
    private Camera mainCamera;
    private GraphSet calculatedResults;
    private GraphSet measuredResults;
    private bool graphShown;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(lineVisualizationControl);
        Assert.IsNotNull(graphControl);

        line = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        tooltipDisplayer = GetComponent<TooltipDisplayer>();
        isCreatingLine = false;
        mainCamera = Camera.main;
        calculatedResults = new GraphSet("Calculated", Color.blue);
        measuredResults = new GraphSet("Measured", Color.green);
        graphShown = true;
    }

    void Update()
    {
        if (isCreatingLine) {
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                if (line.positionCount >= 2) {
                    line.SetPosition(line.positionCount-1, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                    TooltipControl.DisplayTooltip(true, GetLineDistance().ToString("0.0") + "m");
                }

                if (Input.GetMouseButtonDown(0)) {
                    if (line.positionCount == 0) {
                        line.positionCount = 2;
                        line.SetPosition(0, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                        line.SetPosition(1, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                    } else {
                        line.positionCount++;
                        line.SetPosition(line.positionCount-1, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                    }
                }

                if (Input.GetMouseButton(1) && line.positionCount >= 2 && line.GetPosition(0) != line.GetPosition(line.positionCount-1)) {
                    isCreatingLine = false;
                    TooltipControl.DisplayTooltip(false);

                    measuredResults.Clear();
                    lightComputationManager.ObjectDefined(this);
                    SetMeshCollider();
                }
            }
        }
    }

    void OnMouseOver()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << LINE_LAYER)) {
            float distance = 0;
            for (int i=0; i<line.positionCount-1; ++i) {
                Vector3 currentPosition = line.GetPosition(i);
                Vector3 nextPosition = line.GetPosition(i+1);

                if (IsPointOnLine(currentPosition, nextPosition, hit.point)) {
                    float distanceToPoint = distance + Vector3.Distance(currentPosition, hit.point);

                    graphControl.HighlightXLine(distanceToPoint);
                    tooltipDisplayer.SetText("Distance from start of line: " + distanceToPoint.ToString("0.00") + "m");
                }
                distance += Vector3.Distance(currentPosition, nextPosition);
            }
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
        graphControl.Show(show && graphShown);

        if (show) {
            if (IsLineCreated()) {
                lightComputationManager.ObjectDefined(this);
            }
        } else {
            isCreatingLine = false;
            TooltipControl.DisplayTooltip(false);
        }
    }

    public void Draw()
    {
        if (!isCreatingLine) {
            line.positionCount = 0;
            isCreatingLine = true;
            SetMeshCollider();
        }
    }

    public void Erase()
    {
        isCreatingLine = false;
        TooltipControl.DisplayTooltip(false);

        line.positionCount = 0;
        SetMeshCollider();

        calculatedResults.Clear();
        measuredResults.Clear();
        graphControl.Clear();
    }

    public void ImportResults()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Import light results", "", "geojson", false);
        if (paths.Length > 0) {
            string message = "";

            try {
                GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(paths[0]);

                List<Vector3> positions = new List<Vector3>();
                List<float> luminances = new List<float>();
                foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                    GeoJSON.Net.Geometry.Point point = null;
                    if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Point")) {
                        point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                    } else if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiPoint")) {
                        point = (feature.Geometry as GeoJSON.Net.Geometry.MultiPoint).Coordinates[0];
                    }

                    if (point != null && Utils.IsEPSG4326(point.Coordinates) && feature.Properties.ContainsKey("luminance")) {
                        positions.Add(mapManager.GetUnityPositionFromCoordinates(new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude), true));
                        luminances.Add(System.Convert.ToSingle(feature.Properties["luminance"]));
                    }
                }
                
                if (positions.Count > 1) {
                    measuredResults.Clear();
                    for (int i=0; i<positions.Count; ++i) {
                        float distance = 0;
                        if (i > 0) {
                            distance += measuredResults.Abscissas[i-1] + Vector3.Distance(positions[i-1], positions[i]);
                        }
                        measuredResults.Add(distance, luminances[i]);
                    }

                    CreateLineFromPoints(positions);
                    lightComputationManager.ObjectDefined(this);
                } else {
                    message += 
                        "Not enough valid points.\n" +
                        "The GeoJSON file should be made of Point or MultiPoint that have a luminance property.\n" +
                        "The EPSG:4326 coordinate system should be used (longitude from -180° to 180° / latitude from -90° to 90°)."
                    ;
                }
            } catch (System.Exception e) {
                message = e.Message;
            }
            
            if (message != "") {
                DialogControl.CreateDialog(message);
            }
        }
    }

    public void ExportResults()
    {
        if (calculatedResults.Abscissas.Count == 0) {
            DialogControl.CreateDialog("No results to export.");
        } else {
            string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Export light results", "", "light_results", "geojson");
            if (filename != "") {
                List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

                List<Vector3> positions = GetPositionsOfMeasuresAlongLine();

                for (int i=0; i<positions.Count; ++i) {
                    Vector3d coordinate = mapManager.GetCoordinatesFromUnityPosition(positions[i]);
                    GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                        coordinate.latitude,
                        coordinate.longitude,
                        coordinate.altitude
                    ));

                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("luminance", calculatedResults.Ordinates[i]);

                    features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
                }

                GeoJSONParser.FeaturesToFile(filename, features);
            }
        }
    }

    public void ShowVisualizationMethod(bool show)
    {
        graphShown = show;
        graphControl.Show(show);
    }

    public void GetPositionsAnglesAlongObject(out Vector3[] positions, out float[] angles)
    {
        int resolution = lineVisualizationControl.GetResolution();
        positions = new Vector3[resolution+1];
        angles = new float[resolution+1];

        float distanceStep = GetLineDistance() / resolution;

        int currentLinePoint = 0;
        float currentDistance = Vector3.Distance(line.GetPosition(currentLinePoint), line.GetPosition(currentLinePoint+1));
        float currentTotalDistance = currentDistance;
        for (int i=0; i<=resolution; ++i) {
            float distance = i*distanceStep;

            while (currentLinePoint < line.positionCount-2 && distance > currentTotalDistance) {
                currentLinePoint++;
                currentDistance = Vector3.Distance(line.GetPosition(currentLinePoint), line.GetPosition(currentLinePoint+1));
                currentTotalDistance += currentDistance;
            }

            Vector3 currentLinePointPosition = line.GetPosition(currentLinePoint);
            Vector3 nextLinePointPosition = line.GetPosition(currentLinePoint+1);

            float t = (distance - (currentTotalDistance-currentDistance)) / currentDistance;

            positions[i] = Vector3.Lerp(currentLinePointPosition, nextLinePointPosition, t);
            angles[i] = Utils.GetAngleBetweenPositions(
                new Vector2(currentLinePointPosition.x, currentLinePointPosition.z),
                new Vector2(nextLinePointPosition.x, nextLinePointPosition.z)
            );
        }
    }
    
    public void ResultsComputed(Vector3[] positions, float[] luminances)
    {
        calculatedResults.Abscissas = positions.Select((position, index) => {
            return index > 0 ? Vector3.Distance(position, positions[index-1]) : 0;
        }).Aggregate(new List<float>(), (distances, distance) => {
            if (distances.Count > 0) {
                distances.Add(distance + distances.Last());
            } else {
                distances.Add(distance);
            }
            return distances;
        });
        calculatedResults.Ordinates = luminances.ToList();
        graphControl.CreateGraph(new List<GraphSet> { calculatedResults, measuredResults });
    }

    private float GetLineDistance()
    {
        float distance = 0;
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        for (int i=0; i<line.positionCount-1; ++i) {
            distance += Vector3.Distance(positions[i], positions[i+1]);
        }
        return distance;
    }

    private void SetMeshCollider()
    {
        Destroy(meshCollider.sharedMesh);

        Mesh mesh = new Mesh();
        line.BakeMesh(mesh, true);

        // If the line has just been created, the mesh may not be valid
        if (line.positionCount > 1 && line.GetPosition(0) != line.GetPosition(1)) {
            meshCollider.sharedMesh = mesh;
        }
    }

    private bool IsPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        // Round is used to avoid precision issues
        return System.Math.Round(Vector3.Distance(lineStart, point) + Vector3.Distance(lineEnd, point), 2)
            == System.Math.Round(Vector3.Distance(lineStart, lineEnd), 2);
    }

    private bool IsLineCreated()
    {
        return line.positionCount > 1;
    }

    private void CreateLineFromPoints(List<Vector3> points)
    {
        isCreatingLine = false;
        TooltipControl.DisplayTooltip(false);

        line.positionCount = points.Count;
        line.SetPositions(points.Select(point => point + new Vector3(0, LINE_HEIGHT, 0)).ToArray());
        SetMeshCollider();
    }

    private List<Vector3> GetPositionsOfMeasuresAlongLine()
    {
        Vector3[] positions;
        float[] angles;
        GetPositionsAnglesAlongObject(out positions, out angles);
        return positions.Select(p => p - new Vector3(0, LINE_HEIGHT, 0)).ToList();
    }
}