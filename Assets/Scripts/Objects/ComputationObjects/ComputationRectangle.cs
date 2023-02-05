using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
public class ComputationRectangle : MonoBehaviour, IComputationObject
{
    private const float RECTANGLE_HEIGHT = 1;
    [SerializeField] private LightComputationManager lightComputationManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private ObjectVisualizationControl gridVisualizationControl;
    [SerializeField] private HeatmapControl heatmapControl;
    [SerializeField] private GameObject heatmapPositionIndicator;
    private LineRenderer line;
    private bool isCreatingRectangle;
    private bool centerSet;
    private Vector3 center;
    private Camera mainCamera;
    private List<Vector3> positions;
    private List<float> luminances;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(gridVisualizationControl);
        Assert.IsNotNull(heatmapControl);
        Assert.IsNotNull(heatmapPositionIndicator);

        line = GetComponent<LineRenderer>();
        isCreatingRectangle = false;
        centerSet = false;
        mainCamera = Camera.main;
        positions = new List<Vector3>();
        luminances = new List<float>();
    }

    void Update()
    {
        if (isCreatingRectangle) {
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
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
                    secondPoint = mapManager.GetUnityPositionFromCoordinates(mapManager.GetCoordinatesFromUnityPosition(secondPoint), true);
                    thirdPoint = mapManager.GetUnityPositionFromCoordinates(mapManager.GetCoordinatesFromUnityPosition(thirdPoint), true);
                    fourthPoint = mapManager.GetUnityPositionFromCoordinates(mapManager.GetCoordinatesFromUnityPosition(fourthPoint), true);

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

        if (show) {
            if (IsRectangleCreated()) {
                heatmapControl.SetComputing();
                lightComputationManager.ComputeAlongObject(this);
            }
        } else {
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

    public void ShowVisualizationMethod(bool show)
    {
        heatmapControl.Show(show);
    }

    public void GetPositionsAnglesAlongObject(out Vector3[] positions, out float[] angles)
    {
        int resolution = gridVisualizationControl.GetResolution();
        positions = new Vector3[(resolution+1)*(resolution+1)];
        angles = new float[(resolution+1)*(resolution+1)];

        float xStep = Mathf.Abs(line.GetPosition(0).x - line.GetPosition(2).x) / resolution;
        float zStep = Mathf.Abs(line.GetPosition(0).z - line.GetPosition(2).z) / resolution;
        float angle = Utils.GetAngleBetweenPositions(
            new Vector2(line.GetPosition(0).x, line.GetPosition(0).z),
            new Vector2(line.GetPosition(1).x, line.GetPosition(1).z)
        );
        Vector3 origin = new Vector3(
            line.GetPosition(0).x < line.GetPosition(2).x ? line.GetPosition(0).x : line.GetPosition(2).x,
            0,
            line.GetPosition(0).z < line.GetPosition(2).z ? line.GetPosition(0).z : line.GetPosition(2).z
        );

        for (int i=0; i<=resolution; ++i) {
            for (int j=0; j<=resolution; ++j) {
                Vector3 point = origin + new Vector3(xStep * i, 0, zStep * j);

                // Set y coordinate to just above terrain
                point = mapManager.GetUnityPositionFromCoordinates(mapManager.GetCoordinatesFromUnityPosition(point), true) + new Vector3(0, RECTANGLE_HEIGHT, 0);
            
                positions[i*(resolution+1) + j] = point;
                angles[i*(resolution+1) + j] = angle;
            }
        }
    }

    public void ResultsComputed(Vector3[] positions, float[] luminances)
    {
        this.positions = positions.ToList();
        this.luminances = luminances.ToList();

        heatmapControl.SetHeatmap(positions, luminances, () => {
            heatmapControl.SetComputing();
            lightComputationManager.ComputeAlongObject(this);
        });
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
            position = mapManager.GetUnityPositionFromCoordinates(mapManager.GetCoordinatesFromUnityPosition(position), true);

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