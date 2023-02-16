using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(LineRenderer), typeof(TooltipDisplayer))]
public class MeasureManager : MonoBehaviour
{
    [SerializeField] SceneCamerasManager sceneCamerasManager;
    private LineRenderer line;
    private bool isCreatingLine;

    void Awake()
    {
        Assert.IsNotNull(sceneCamerasManager);

        line = GetComponent<LineRenderer>();
        isCreatingLine = false;
    }

    void Update()
    {
        if (isCreatingLine) {
            RaycastHit hit;
            if (Physics.Raycast(sceneCamerasManager.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity)) {
                if (line.positionCount >= 2) {
                    line.SetPosition(line.positionCount-1, hit.point);
                    TooltipControl.DisplayTooltip(true, Utils.GetLineDistance(line).ToString("0.0") + "m");
                }

                if (Input.GetMouseButtonDown(0)) {
                    if (line.positionCount == 0) {
                        line.positionCount = 2;
                        line.SetPosition(0, hit.point);
                        line.SetPosition(1, hit.point);
                    } else {
                        line.positionCount++;
                        line.SetPosition(line.positionCount-1, hit.point);
                    }
                }

                if (Input.GetMouseButton(1) && line.positionCount >= 2 && line.GetPosition(0) != line.GetPosition(line.positionCount-1)) {
                    isCreatingLine = false;
                    line.positionCount = 0;
                    TooltipControl.DisplayTooltip(false);
                }
            }
        }
    }

    public void Measure()
    {
        isCreatingLine = true;
    }
}