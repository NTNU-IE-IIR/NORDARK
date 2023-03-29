using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
public class ComputationRectangle : MonoBehaviour, IComputationObject
{
    private const float RECTANGLE_HEIGHT = 1;
    private const int MIN_RESOLUTION = 2;
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private SceneCamerasManager sceneCamerasManager;
    [SerializeField] private ObjectVisualizationControl gridVisualizationControl;
    [SerializeField] private HeatmapControl heatmapControl;
    [SerializeField] private GameObject heatmapPositionIndicator;
    private LineRenderer line;
    private bool isCreatingRectangle;
    private bool centerSet;
    private Vector3 center;
    private List<Vector3> positions;
    private List<List<float>> luminances;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(gridVisualizationControl);
        Assert.IsNotNull(heatmapControl);
        Assert.IsNotNull(heatmapPositionIndicator);

        line = GetComponent<LineRenderer>();
        isCreatingRectangle = false;
        centerSet = false;
        positions = new List<Vector3>();
        luminances = new List<List<float>>();
    }

    void Update()
    {
        if (isCreatingRectangle) {
            RaycastHit hit;
            if (Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << TerrainManager.TERRAIN_LAYER)) {
                if (Input.GetMouseButton(0)) {
                    center = hit.point;
                    centerSet = true;
                }

                if (centerSet) {
                    Vector3 firstPoint = hit.point;
                    Vector3 thirdPoint = firstPoint + 2*(center - firstPoint);
                    Vector3 secondPoint = new Vector3(thirdPoint.x, 0, firstPoint.z);
                    Vector3 fourthPoint = new Vector3(firstPoint.x, 0, thirdPoint.z);

                    // Set point altitudes
                    secondPoint = terrainManager.GetUnityPositionFromCoordinates(terrainManager.GetCoordinatesFromUnityPosition(secondPoint), true);
                    thirdPoint = terrainManager.GetUnityPositionFromCoordinates(terrainManager.GetCoordinatesFromUnityPosition(thirdPoint), true);
                    fourthPoint = terrainManager.GetUnityPositionFromCoordinates(terrainManager.GetCoordinatesFromUnityPosition(fourthPoint), true);

                    line.SetPosition(0, firstPoint + new Vector3(0, RECTANGLE_HEIGHT, 0));
                    line.SetPosition(1, secondPoint + new Vector3(0, RECTANGLE_HEIGHT, 0));
                    line.SetPosition(2, thirdPoint + new Vector3(0, RECTANGLE_HEIGHT, 0));
                    line.SetPosition(3, fourthPoint + new Vector3(0, RECTANGLE_HEIGHT, 0));
                    
                    TooltipControl.DisplayTooltip(true, 
                        "Width:" + Mathf.Abs(firstPoint.x - thirdPoint.x).ToString("0.0") + "m\n" +
                        "Length:" + Mathf.Abs(firstPoint.z - thirdPoint.z).ToString("0.0") + "m"
                    );
                }

                if (Input.GetMouseButton(1) && line.GetPosition(0) != line.GetPosition(1)) {
                    isCreatingRectangle = false;
                    TooltipControl.DisplayTooltip(false);
                    heatmapControl.SetComputing();
                    lightComputationManager.ComputeAlongObject(this);
                }
            }
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);

        if (!show) {
            isCreatingRectangle = false;
            TooltipControl.DisplayTooltip(false);
        }
    }

    public void Draw()
    {
        isCreatingRectangle = true;
        centerSet = false;
        line.SetPosition(0, new Vector3(0, 0, 0));
        line.SetPosition(1, new Vector3(0, 0, 0));
        line.SetPosition(2, new Vector3(0, 0, 0));
        line.SetPosition(3, new Vector3(0, 0, 0));
    }

    public void Erase()
    {
        isCreatingRectangle = false;
        TooltipControl.DisplayTooltip(false);
        heatmapControl.SetWaitingForDefinition();
        positions.Clear();
        luminances.Clear();
    }

    public void ImportResults()
    {}

    public void ExportResultsGeoJSON()
    {
        lightComputationManager.ExportResultsGeoJSON(positions, luminances);
    }

    public void ExportResultsCSV()
    {
        lightComputationManager.ExportResultsCSV(positions, luminances);
    }

    public void DisplayValues(bool display)
    {
        heatmapControl.DisplayValues(display);
    }

    public void ShowVisualizationMethod(bool show)
    {
        heatmapControl.Show(show);
    }

    public void GetPositionsAnglesAlongObject(out Vector3[] positions, out float[] angles)
    {
        int resolution = Mathf.Max(gridVisualizationControl.GetResolution(), MIN_RESOLUTION);
        positions = new Vector3[resolution*resolution];
        angles = new float[resolution*resolution];

        float xStep = Mathf.Abs(line.GetPosition(0).x - line.GetPosition(2).x) / (resolution-1);
        float zStep = Mathf.Abs(line.GetPosition(0).z - line.GetPosition(2).z) / (resolution-1);
        float angle = Utils.GetAngleBetweenPositions(
            new Vector2(line.GetPosition(0).x, line.GetPosition(0).z),
            new Vector2(line.GetPosition(1).x, line.GetPosition(1).z)
        );
        Vector3 origin = new Vector3(
            line.GetPosition(0).x < line.GetPosition(2).x ? line.GetPosition(0).x : line.GetPosition(2).x,
            0,
            line.GetPosition(0).z < line.GetPosition(2).z ? line.GetPosition(0).z : line.GetPosition(2).z
        );

        for (int i=0; i<resolution; ++i) {
            for (int j=0; j<resolution; ++j) {
                Vector3 point = origin + new Vector3(xStep * i, 0, zStep * j);

                // Set y coordinate to just above terrain
                point = terrainManager.GetUnityPositionFromCoordinates(terrainManager.GetCoordinatesFromUnityPosition(point), true) + new Vector3(0, RECTANGLE_HEIGHT, 0);
            
                positions[i*resolution + j] = point;
                angles[i*resolution + j] = angle;
            }
        }
    }

    public void ResultsComputed(Vector3[] positions, float[,] luminances)
    {
        this.positions = positions.ToList();
        this.luminances = Enumerable.Range(0, luminances.GetLength(0))
            .Select(i => {
                return Enumerable.Range(0, luminances.GetLength(1))
                    .Select(j => luminances[i, j])
                    .ToList();
            })
            .ToList();

        float yMinimum = gridVisualizationControl.isMinAuto() ?
            IComputationObject.EXTREMUM_DEFAULT_VALUE :
            gridVisualizationControl.GetMinValue()
        ;
        float yMaximum = gridVisualizationControl.isMaxAuto() ?
            IComputationObject.EXTREMUM_DEFAULT_VALUE :
            gridVisualizationControl.GetMaxValue()
        ;

        heatmapControl.SetHeatmap(positions, this.luminances[0].ToArray(), () => {
            heatmapControl.SetComputing();
            lightComputationManager.ComputeAlongObject(this);
        }, yMinimum, yMaximum);
    }

    public void OnMouseOver(Vector2 normalizedPosition)
    {
        if (positions.Count > 1) {
            Vector3 position = new Vector3(
                Mathf.Lerp(positions.First().x, positions.Last().x, normalizedPosition.y),
                0,
                Mathf.Lerp(positions.First().z, positions.Last().z, normalizedPosition.x)
            );

            // Set y coordinate to terrain height
            position = terrainManager.GetUnityPositionFromCoordinates(terrainManager.GetCoordinatesFromUnityPosition(position), true);

            heatmapPositionIndicator.SetActive(true);
            heatmapPositionIndicator.transform.position = position;
        }
    }

    public void OnMouseExit()
    {
        heatmapPositionIndicator.SetActive(false);
    }

    private bool IsRectangleCreated()
    {
        return line.GetPosition(0) != line.GetPosition(1) && line.GetPosition(1) != line.GetPosition(2) && line.GetPosition(2) != line.GetPosition(3);
    }
}