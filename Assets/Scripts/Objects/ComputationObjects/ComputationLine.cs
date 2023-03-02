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
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private ObjectVisualizationControl lineVisualizationControl;
    [SerializeField] private GraphControl graphControl;
    private LineRenderer line;
    private MeshCollider meshCollider;
    private TooltipDisplayer tooltipDisplayer;
    private bool isCreatingLine;
    private GraphSet importedResults;
    private List<GraphSet> calculatedResults;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(lineVisualizationControl);
        Assert.IsNotNull(graphControl);

        line = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        tooltipDisplayer = GetComponent<TooltipDisplayer>();
        isCreatingLine = false;
        importedResults = new GraphSet("Imported", Color.green);
        calculatedResults = new List<GraphSet>();
    }

    void Update()
    {
        if (isCreatingLine) {
            RaycastHit hit;
            if (Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                if (line.positionCount >= 2) {
                    line.SetPosition(line.positionCount-1, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                    float distance = Utils.GetLineDistance(line);
                    Vector3 lastLineVector = line.GetPosition(line.positionCount-1) - line.GetPosition(line.positionCount-2);
                    float angle = Utils.GetAngleBetweenPositions(new Vector2(lastLineVector.x, lastLineVector.z), new Vector2(0, 1));

                    angle = 360 - (Mathf.Rad2Deg * Mathf.Atan2(lastLineVector.z, lastLineVector.x) + 270 ) % 360;

                    TooltipControl.DisplayTooltip(true, distance.ToString("0.0") + "m\n" + angle.ToString("0.0") + "°");
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

                    importedResults.Clear();
                    lightComputationManager.ComputeAlongObject(this);
                    SetMeshCollider();
                }
            }
        }
    }

    void OnMouseOver()
    {
        RaycastHit hit;
        if (Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << LINE_LAYER)) {
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

        if (show) {
            if (IsLineCreated()) {
                lightComputationManager.ComputeAlongObject(this);
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
        importedResults.Clear();
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
                    importedResults.Clear();
                    for (int i=0; i<positions.Count; ++i) {
                        float distance = 0;
                        if (i > 0) {
                            distance += importedResults.Abscissas[i-1] + Vector3.Distance(positions[i-1], positions[i]);
                        }
                        importedResults.Add(distance, luminances[i]);
                    }

                    CreateLineFromPoints(positions);
                    lightComputationManager.ComputeAlongObject(this);
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

    public void ExportResultsGeoJSON()
    {
        lightComputationManager.ExportResultsGeoJSON(GetPositionsOfMeasuresAlongLine(), calculatedResults.Select(graphSet => graphSet.Ordinates).ToList());
    }

    public void ExportResultsCSV()
    {
        lightComputationManager.ExportResultsCSV(GetPositionsOfMeasuresAlongLine(), calculatedResults.Select(graphSet => graphSet.Ordinates).ToList());
    }
    
    public void DisplayValues(bool display)
    {}

    public void ShowVisualizationMethod(bool show)
    {
        graphControl.Show(show);
    }

    public void GetPositionsAnglesAlongObject(out Vector3[] positions, out float[] angles)
    {
        int resolution = lineVisualizationControl.GetResolution();
        positions = new Vector3[resolution+1];
        angles = new float[resolution+1];

        float distanceStep = Utils.GetLineDistance(line) / resolution;

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

            positions[i] = Vector3.Lerp(
                currentLinePointPosition,
                nextLinePointPosition,
                (distance - (currentTotalDistance-currentDistance)) / currentDistance
            );
            angles[i] = Utils.GetAngleBetweenPositions(
                new Vector2(currentLinePointPosition.x, currentLinePointPosition.z),
                new Vector2(nextLinePointPosition.x, nextLinePointPosition.z)
            );
        }
    }
    
    public void ResultsComputed(Vector3[] positions, float[,] luminances)
    {
        List<float> distances = positions.Select((position, index) => {
            return index > 0 ? Vector3.Distance(position, positions[index-1]) : 0;
        }).Aggregate(new List<float>(), (distances, distance) => {
            if (distances.Count > 0) {
                distances.Add(distance + distances.Last());
            } else {
                distances.Add(distance);
            }
            return distances;
        });
        List<GraphSet> graphSets = new List<GraphSet>();

        for (int i=0; i<luminances.GetLength(0); ++i) {
            GraphSet results = new GraphSet("Config " + i.ToString(), Random.ColorHSV(0, 1, 1, 1, 0.5f, 1, 1, 1));
            results.Abscissas = distances;
            results.Ordinates = Enumerable.Range(0, luminances.GetLength(1))
                .Select(x => luminances[i, x])
                .ToList();

            graphSets.Add(results);

            calculatedResults.Add(results);
        }

        graphSets.Add(importedResults);

        float yMinimum = lineVisualizationControl.isMinAuto() ?
            IComputationObject.EXTREMUM_DEFAULT_VALUE :
            lineVisualizationControl.GetMinValue()
        ;
        float yMaximum = lineVisualizationControl.isMaxAuto() ?
            IComputationObject.EXTREMUM_DEFAULT_VALUE :
            lineVisualizationControl.GetMaxValue()
        ;

        graphControl.CreateGraph(graphSets, () => {
            lightComputationManager.ComputeAlongObject(this);
        }, yMinimum, yMaximum);
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