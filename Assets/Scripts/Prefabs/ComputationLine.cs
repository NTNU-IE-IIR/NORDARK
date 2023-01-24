using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(TooltipDisplayer))]
public class ComputationLine : MonoBehaviour
{
    private const float LINE_HEIGHT = 2;
    private const int LINE_LAYER = 11;
    [SerializeField] private LightComputationManager lightComputationManager;
    private LineRenderer line;
    private MeshCollider meshCollider;
    private TooltipDisplayer tooltipDisplayer;
    private bool isCreatingLine;
    private Camera mainCamera;

    void Awake()
    {
        Assert.IsNotNull(lightComputationManager);

        line = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        tooltipDisplayer = GetComponent<TooltipDisplayer>();
        isCreatingLine = false;
        mainCamera = Camera.main;
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
                    lightComputationManager.LineDefined();
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

                    lightComputationManager.LineHighlighted(distanceToPoint);
                    tooltipDisplayer.SetText("Distance from start of line: " + distanceToPoint.ToString("0.00") + "m");
                }
                distance += Vector3.Distance(currentPosition, nextPosition);
            }
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);

        if (!show) {
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
    }

    public bool IsLineCreated()
    {
        return line.positionCount > 1;
    }

    public void CreateLineFromPoints(List<Vector3> points)
    {
        Erase();
        line.positionCount = points.Count;
        line.SetPositions(points.Select(point => point + new Vector3(0, LINE_HEIGHT, 0)).ToArray());
        SetMeshCollider();
    }

    public void GetPositionsAnglesDistancesAlongLine(int numberOfPositions, out Vector3[] positions, out float[] angles, out float[] distances)
    {
        positions = new Vector3[numberOfPositions+1];
        angles = new float[numberOfPositions+1];
        distances = new float[numberOfPositions+1];

        float distanceStep = GetLineDistance() / numberOfPositions;

        int currentLinePoint = 0;
        float currentDistance = Vector3.Distance(line.GetPosition(currentLinePoint), line.GetPosition(currentLinePoint+1));
        float currentTotalDistance = currentDistance;
        for (int i=0; i<=numberOfPositions; ++i) {
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
            distances[i] = distance;
        }
    }

    public List<Vector3> GetPositionsOfMeasuresAlongLine(int numberOfPositions)
    {
        Vector3[] positions;
        float[] angles;
        float[] distance;
        GetPositionsAnglesDistancesAlongLine(numberOfPositions, out positions, out angles, out distance);
        return positions.Select(p => p - new Vector3(0, LINE_HEIGHT, 0)).ToList();
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

    private bool IsPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        // Round is used to avoid precision issues
        return System.Math.Round(Vector3.Distance(lineStart, point) + Vector3.Distance(lineEnd, point), 2)
            == System.Math.Round(Vector3.Distance(lineStart, lineEnd), 2);
    }
}
