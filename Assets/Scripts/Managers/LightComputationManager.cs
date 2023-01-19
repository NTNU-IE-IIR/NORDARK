using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationManager : MonoBehaviour
{
    private const float LINE_HEIGHT = 2;
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
    [SerializeField] private LineRenderer line;
    private bool isCreatingLine;
    private int indexOfLuminanceMaxShader;
    private List<float> calculatedResults;
    private List<float> measuredResults;

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
        Assert.IsNotNull(line);

        isCreatingLine = false;
        indexOfLuminanceMaxShader = luminanceMaxShader.FindKernel("CSMain");
        calculatedResults = new List<float>();
        measuredResults = new List<float>();
    }

    void Update()
    {
        if (isCreatingLine) {
            if (line.positionCount == 2) {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                    line.SetPosition(1, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << MapManager.UNITY_LAYER_MAP)) {
                    if (line.positionCount == 0) {
                        line.positionCount = 2;
                        line.SetPosition(0, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                        line.SetPosition(1, hit.point + new Vector3(0, LINE_HEIGHT, 0));
                    } else {
                        isCreatingLine = false;
                        measuredResults.Clear();
                        StartCoroutine(ComputeAlongLine());
                    }
                }
            }
        }
    }

    public void Open()
    {
        graphControl.Show(true);
        line.gameObject.SetActive(true);
        if (line.positionCount == 2) {
            StartCoroutine(ComputeAlongLine());
        }
    }

    public void Close()
    {
        graphControl.Show(false);
        line.gameObject.SetActive(false);
        isCreatingLine = false;
    }

    public void OnLocationChanged()
    {
        isCreatingLine = false;
        line.positionCount = 0;

        calculatedResults.Clear();
        measuredResults.Clear();
        
        graphControl.Clear();
    }

    public void DisplayLightResults(bool display)
    {
        luminanceMapPass.SetActive(display);
        legend.SetActive(display);
    }

    public void DrawLine()
    {
        if (!isCreatingLine) {
            line.positionCount = 0;
            isCreatingLine = true;
        }
    }

    public void ImportResults()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Import light results", "", "geojson", false);
        if (paths.Length > 0) {
            string message = "";

            try {
                GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(paths[0]);

                List<Vector3> positions = new List<Vector3>();
                measuredResults.Clear();

                foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                    GeoJSON.Net.Geometry.Point point = null;
                    if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Point")) {
                        point = feature.Geometry as GeoJSON.Net.Geometry.Point;
                    } else if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.MultiPoint")) {
                        point = (feature.Geometry as GeoJSON.Net.Geometry.MultiPoint).Coordinates[0];
                    }

                    if (point != null && Utils.IsEPSG4326(point.Coordinates) && feature.Properties.ContainsKey("luminance")) {
                        positions.Add(mapManager.GetUnityPositionFromCoordinates(new Vector3d(point.Coordinates.Latitude, point.Coordinates.Longitude), true));
                        measuredResults.Add(System.Convert.ToSingle(feature.Properties["luminance"]));
                    }
                }

                if (positions.Count > 1) {
                    isCreatingLine = false;
                    line.positionCount = 2;
                    line.SetPosition(0, positions.First() + new Vector3(0, LINE_HEIGHT, 0));
                    line.SetPosition(1, positions.Last() + new Vector3(0, LINE_HEIGHT, 0));

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
        if (calculatedResults.Count == 0) {
            dialogControl.CreateInfoDialog("No results to export.");
        } else {
            string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Export light results", "", "light_results", "geojson");
            if (filename != "") {
                List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

                for (int i=0; i<=COMPUTATION_RESOLUTION; ++i) {
                    Vector3d coordinate = mapManager.GetCoordinatesFromUnityPosition(Vector3.Lerp(line.GetPosition(0), line.GetPosition(1), (float) i / COMPUTATION_RESOLUTION));
                    GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                        coordinate.latitude,
                        coordinate.longitude,
                        coordinate.altitude
                    ));

                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("luminance", calculatedResults[i]);

                    features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
                }

                GeoJSONParser.FeaturesToFile(filename, features);
            }
        }
    }

    private IEnumerator ComputeAlongLine()
    {
        luminancePass.SetActive(true);

        RenderTexture luminanceTexture = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        luminanceTexture.enableRandomWrite = true;
        luminanceTexture.Create();

        Camera luminanceCamera = Instantiate(luminanceCameraPrefab, transform).GetComponent<Camera>();
        luminanceCamera.targetTexture = luminanceTexture;
        luminanceCamera.farClipPlane = LINE_HEIGHT + 1;
        luminanceCamera.transform.eulerAngles = new Vector3(
            luminanceCamera.transform.eulerAngles.x,
            luminanceCamera.transform.eulerAngles.y,
            Utils.GetAngleBetweenPositions(
                new Vector2(line.GetPosition(0).x, line.GetPosition(0).z),
                new Vector2(line.GetPosition(1).x, line.GetPosition(1).z)
            )
        );
        
        // Skip frame to let luminancePass turning on
        yield return null;

        calculatedResults.Clear();
        for (int i=0; i<=COMPUTATION_RESOLUTION; ++i) {
            luminanceCamera.transform.position = Vector3.Lerp(line.GetPosition(0), line.GetPosition(1), (float) i / COMPUTATION_RESOLUTION);
        
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
            calculatedResults.Add(result[0] / LUMINANCE_RESOLUTION);
            computeBuffer.Release();
        }

        luminanceTexture.Release();
        Destroy(luminanceCamera.gameObject);
        luminancePass.SetActive(false);

        graphControl.CreateGraph(
            new List<List<float>> { calculatedResults, measuredResults },
            new List<Color> { Color.blue, Color.green },
            new List<string> { "Calculated", "Measured" },
            i => {
                return Vector3.Distance(line.GetPosition(0), Vector3.Lerp(line.GetPosition(0), line.GetPosition(1), (float) i / COMPUTATION_RESOLUTION)).ToString("0.0");
            }
        );
    }
}