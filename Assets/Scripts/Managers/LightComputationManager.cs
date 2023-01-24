using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationManager : MonoBehaviour
{
    private const int MASK_TEXTURE_SIZE = 32;
    private const int COMPUTATION_RESOLUTION = 20;
    private const float LUMINANCE_RESOLUTION = 100;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private LightComputationControl lightComputationControl;
    [SerializeField] private GraphControl graphControl;
    [SerializeField] private DialogControl dialogControl;
    [SerializeField] private GameObject luminanceMapPass;
    [SerializeField] private GameObject luminancePass;
    [SerializeField] private GameObject legend;
    [SerializeField] private GameObject luminanceCameraPrefab;
    [SerializeField] private ComputeShader luminanceMaxShader;
    [SerializeField] private ComputationLine computationLine;
    private int indexOfLuminanceMaxShader;
    private GraphSet calculatedResults;
    private GraphSet measuredResults;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(lightComputationControl);
        Assert.IsNotNull(graphControl);
        Assert.IsNotNull(dialogControl);
        Assert.IsNotNull(luminanceMapPass);
        Assert.IsNotNull(luminancePass);
        Assert.IsNotNull(legend);
        Assert.IsNotNull(luminanceCameraPrefab);
        Assert.IsNotNull(luminanceMaxShader);
        Assert.IsNotNull(computationLine);

        indexOfLuminanceMaxShader = luminanceMaxShader.FindKernel("CSMain");
        calculatedResults = new GraphSet("Calculated", Color.blue);
        measuredResults = new GraphSet("Measured", Color.green);
    }

    public void OnLocationChanged()
    {
        computationLine.Erase();

        calculatedResults.Clear();
        measuredResults.Clear();
        
        graphControl.Clear();
    }

    public void Open()
    {
        graphControl.Show(true);
        computationLine.Show(true);
        if (computationLine.IsLineCreated()) {
            StartCoroutine(ComputeAlongLine());
        }
    }

    public void Close()
    {
        graphControl.Show(false);
        computationLine.Show(false);
    }

    public void LineDefined()
    {
        measuredResults.Clear();
        StartCoroutine(ComputeAlongLine());
    }

    public void DisplayLuminanceMap(bool display)
    {
        luminanceMapPass.SetActive(display);
        legend.SetActive(display);
    }

    public void DrawLine()
    {
        computationLine.Draw();
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

                    computationLine.CreateLineFromPoints(positions);
                    StartCoroutine(ComputeAlongLine());
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
                dialogControl.CreateInfoDialog(message);
            }
        }
    }

    public void ExportResults()
    {
        if (calculatedResults.Abscissas.Count == 0) {
            dialogControl.CreateInfoDialog("No results to export.");
        } else {
            string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Export light results", "", "light_results", "geojson");
            if (filename != "") {
                List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

                List<Vector3> positions = computationLine.GetPositionsOfMeasuresAlongLine(COMPUTATION_RESOLUTION);

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

    public void LineHighlighted(float distance)
    {
        graphControl.HighlightXLine(distance);
    }

    private IEnumerator ComputeAlongLine()
    {
        luminancePass.SetActive(true);

        RenderTexture luminanceTexture = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        luminanceTexture.enableRandomWrite = true;
        luminanceTexture.Create();

        Camera luminanceCamera = Instantiate(luminanceCameraPrefab, transform).GetComponent<Camera>();
        luminanceCamera.targetTexture = luminanceTexture;

        Vector3[] positions;
        float[] angles;
        float[] distances;
        computationLine.GetPositionsAnglesDistancesAlongLine(COMPUTATION_RESOLUTION, out positions, out angles, out distances);
        
        // Skip frame to let luminancePass turning on
        yield return null;

        calculatedResults.Clear();
        for (int i=0; i<positions.Length; ++i) {
            luminanceCamera.transform.position = positions[i];
            luminanceCamera.transform.eulerAngles = new Vector3(
                luminanceCamera.transform.eulerAngles.x,
                luminanceCamera.transform.eulerAngles.y,
                angles[i]
            );
        
            // Skip frame to render to camera
            yield return null;

            int[] result = new int[1];
            result[0] = 0;

            ComputeBuffer computeBuffer = new ComputeBuffer(1, 4);
            computeBuffer.SetData(result);

            luminanceMaxShader.SetTexture(indexOfLuminanceMaxShader, "Texture", luminanceTexture);
            luminanceMaxShader.SetFloat("LuminanceResolution", LUMINANCE_RESOLUTION);
            luminanceMaxShader.SetBuffer(indexOfLuminanceMaxShader, "Result", computeBuffer);
            luminanceMaxShader.Dispatch(indexOfLuminanceMaxShader, luminanceTexture.width / 32, luminanceTexture.height / 32, 1);

            computeBuffer.GetData(result);                        
            calculatedResults.Add(distances[i], result[0] / LUMINANCE_RESOLUTION);
            computeBuffer.Release();
        }

        luminanceTexture.Release();
        Destroy(luminanceCamera.gameObject);
        luminancePass.SetActive(false);

        graphControl.CreateGraph(new List<GraphSet> { calculatedResults, measuredResults });
    }
}