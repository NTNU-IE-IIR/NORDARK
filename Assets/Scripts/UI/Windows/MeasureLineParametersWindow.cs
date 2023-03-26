using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using TMPro;

public class MeasureLineParametersWindow : MonoBehaviour
{
    [SerializeField] private ComputationLine computationLine;
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private Button close;
    [SerializeField] private TMP_Text selectedLightPoleName;
    [SerializeField] private TMP_InputField lineLengthInput;
    [SerializeField] private TMP_InputField lineAngleInput;
    [SerializeField] private TMP_InputField distanceBetweenPointsInput;
    [SerializeField] private Button createMeasureLine;
    private LightPole selectedLightPole;

    void Awake()
    {
        Assert.IsNotNull(computationLine);
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(close);
        Assert.IsNotNull(selectedLightPoleName);
        Assert.IsNotNull(lineLengthInput);
        Assert.IsNotNull(lineAngleInput);
        Assert.IsNotNull(distanceBetweenPointsInput);
        Assert.IsNotNull(createMeasureLine);
    }

    void Start()
    {
        close.onClick.AddListener(Close);
        createMeasureLine.onClick.AddListener(CreateMeasureLine);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            LightPole lightPole = lightPolesManager.GetLightPolePointedByCursor();
            if (lightPole != null) {
                selectedLightPole = lightPole;
                selectedLightPoleName.text = "Selected light pole: " + selectedLightPole.Name;
                createMeasureLine.interactable = true;
            }
        }
    }

    public void Close()
    {
        selectedLightPole = null;
        selectedLightPoleName.text = "No selected light pole";
        createMeasureLine.interactable = false;
        gameObject.SetActive(false);
    }

    private void CreateMeasureLine()
    {
        if (selectedLightPole != null) {
            float lineLength = float.Parse(lineLengthInput.text);
            float lineAngle = float.Parse(lineAngleInput.text);
            float distanceBetweenPoints = float.Parse(distanceBetweenPointsInput.text);

            // Make lineLength a multiple of distanceBetweenPoints
            if (lineLength % distanceBetweenPoints != 0) {
                lineLength += (1 - (lineLength % distanceBetweenPoints) / distanceBetweenPoints) * distanceBetweenPoints;
            }

            // Correct angle (see Update() function of ComputationLine.cs)
            lineAngle = (360 - (lineAngle + 270)) % 360;

            Vector3 startingPosition = selectedLightPole.GameObject.transform.position;
            Vector3 endingPosition = startingPosition + lineLength * new Vector3(
                Mathf.Cos(Mathf.Deg2Rad * lineAngle),
                0,
                Mathf.Sin(Mathf.Deg2Rad * lineAngle)
            );

            // Set y coordinate to ground altitude
            endingPosition = terrainManager.GetUnityPositionFromCoordinates(
                terrainManager.GetCoordinatesFromUnityPosition(endingPosition),
                true
            );

            computationLine.CreateLineFromPositionsAndCompute(
                startingPosition,
                endingPosition,
                1 + (int) (lineLength / distanceBetweenPoints)
            );
        }
    }
}
