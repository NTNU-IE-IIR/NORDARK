using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationManager : MonoBehaviour
{
    private const int MASK_TEXTURE_SIZE = 32;
    private const float LUMINANCE_RESOLUTION = 100;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private VegetationManager vegetationManager;
    [SerializeField] private LightPolesManager lightPolesManager;
    [SerializeField] private LightConfigurationsManager lightConfigurationsManager;
    [SerializeField] private ObjectVisualizationControl lineVisualizationControl;
    [SerializeField] private ObjectVisualizationControl gridVisualizationControl;
    [SerializeField] private GameObject luminanceCameraPrefab;
    [SerializeField] private GameObject luminancePassAndVolume;
    [SerializeField] private ComputeShader luminanceSumShader;
    private int indexOfLuminanceSumShader;
    private List<IComputationObject> computationObjects;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(lightPolesManager);
        Assert.IsNotNull(lightConfigurationsManager);
        Assert.IsNotNull(lineVisualizationControl);
        Assert.IsNotNull(gridVisualizationControl);
        Assert.IsNotNull(luminanceCameraPrefab);
        Assert.IsNotNull(luminancePassAndVolume);
        Assert.IsNotNull(luminanceSumShader);

        indexOfLuminanceSumShader = luminanceSumShader.FindKernel("CSMain");
        computationObjects = new List<IComputationObject> {
            lineVisualizationControl.GetComputationObject(),
            gridVisualizationControl.GetComputationObject()
        };
    }

    public void OnLocationChanged()
    {
        foreach (IComputationObject computationObject in computationObjects) {
            computationObject.Erase();
        }
    }

    public void Open()
    {
        foreach (IComputationObject computationObject in computationObjects) {
            computationObject.Show(true);
        }
    }

    public void Close()
    {
        foreach (IComputationObject computationObject in computationObjects) {
            computationObject.Show(false);
        }
    }

    public void ComputeAlongObject(IComputationObject computationObject)
    {
        StartCoroutine(Compute(computationObject));
    }

    public void ExportResultsGeoJSON(List<Vector3> positions, List<List<float>> luminances)
    {
        if (luminances.Count == 0) {
            DialogControl.CreateDialog("No results to export.");
        } else {
            string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Export light results", "", "light_results", "geojson");
            if (filename != "") {
                List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

                for (int i=0; i<luminances.Count; ++i) {
                    float distanceFromOrigin = 0;
                    for (int j=0; j<positions.Count; ++j) {
                        if (j > 0) {
                            distanceFromOrigin += Vector3.Distance(positions[j-1], positions[j]);
                        }

                        Vector3d coordinate = mapManager.GetCoordinatesFromUnityPosition(positions[j]);
                        GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                            coordinate.latitude,
                            coordinate.longitude,
                            coordinate.altitude
                        ));

                        Dictionary<string, object> properties = new Dictionary<string, object>();
                        properties.Add("luminance", luminances[i][j]);
                        properties.Add("ground", mapManager.GetGroundFromPosition(positions[j]));
                        properties.Add("distanceFromOrigin", distanceFromOrigin);
                        properties.Add("config", i);

                        features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
                    }
                }

                GeoJSONParser.FeaturesToFile(filename, features);
            }
        }
    }

    public void ExportResultsCSV(List<Vector3> positions, List<List<float>> luminances)
    {
        if (luminances.Count == 0) {
            DialogControl.CreateDialog("No results to export.");
        } else {
            string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Export light results", "", "light_results", "csv");
            if (filename != "") {
                string content = "latitude,longitude,altitude(m),distanceFromOrigin(m),luminance(cd/m2),ground,config\n";

                for (int i=0; i<luminances.Count; ++i) {
                    float distanceFromOrigin = 0;
                    for (int j=0; j<positions.Count; ++j) {
                        if (j > 0) {
                            distanceFromOrigin += Vector3.Distance(positions[j-1], positions[j]);
                        }

                        Vector3d coordinate = mapManager.GetCoordinatesFromUnityPosition(positions[j]);
                        content += 
                            coordinate.latitude.ToString() + "," +
                            coordinate.longitude.ToString() + "," +
                            coordinate.altitude + "," +
                            distanceFromOrigin + "," +
                            luminances[i][j].ToString() + "," +
                            mapManager.GetGroundFromPosition(positions[j]) + "," +
                            i.ToString() + "\n";
                    }
                }

                System.IO.StreamWriter sw = new System.IO.StreamWriter(filename);
                sw.WriteLine(content);
                sw.Close();
            }
        }
    }

    private IEnumerator Compute(IComputationObject computationObject)
    {
        luminancePassAndVolume.SetActive(true);

        LuminanceCamera luminanceCamera = Instantiate(luminanceCameraPrefab, transform).GetComponent<LuminanceCamera>();
        luminanceCamera.Create(
            MASK_TEXTURE_SIZE,
            vegetationManager,
            lightPolesManager
        );

        int numberOfConfigurations = lightConfigurationsManager.GetCurrentNumberOfConfigurations();
        Vector3[] positions;
        float[] angles;
        computationObject.GetPositionsAnglesAlongObject(out positions, out angles);
        float[,] luminances = new float[numberOfConfigurations, positions.Length];

        // Skip frame to let luminancePassAndVolume turning on
        yield return null;

        for (int i = 0; i<numberOfConfigurations; ++i) {
            luminanceCamera.SetConfigurationIndex(i);

            for (int j=0; j<positions.Length; ++j) {
                luminanceCamera.SetPositionAndAngle(positions[j], angles[j]);
            
                // Skip frame to render to camera
                yield return null;

                // Skip frame to render vegetation, needed only for the first point
                if (j ==0) {
                    yield return null;
                }

                int[] result = new int[1];
                result[0] = 0;

                ComputeBuffer computeBuffer = new ComputeBuffer(1, 4);
                computeBuffer.SetData(result);

                RenderTexture luminanceTexture = luminanceCamera.GetTexture();

                luminanceSumShader.SetTexture(indexOfLuminanceSumShader, "Texture", luminanceTexture);
                luminanceSumShader.SetFloat("LuminanceResolution", LUMINANCE_RESOLUTION);
                luminanceSumShader.SetBuffer(indexOfLuminanceSumShader, "Result", computeBuffer);
                luminanceSumShader.Dispatch(indexOfLuminanceSumShader, luminanceTexture.width / 32, luminanceTexture.height / 32, 1);

                computeBuffer.GetData(result);
                luminances[i, j] = result[0] / LUMINANCE_RESOLUTION / (MASK_TEXTURE_SIZE * MASK_TEXTURE_SIZE);
                computeBuffer.Release();
            }
        }

        Destroy(luminanceCamera.gameObject);
        luminancePassAndVolume.SetActive(false);

        computationObject.ResultsComputed(positions, luminances);
    }
}