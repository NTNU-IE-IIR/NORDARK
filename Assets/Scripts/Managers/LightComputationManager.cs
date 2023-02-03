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
    [SerializeField] private ObjectVisualizationControl lineVisualizationControl;
    [SerializeField] private ObjectVisualizationControl gridVisualizationControl;
    [SerializeField] private GameObject luminancePass;
    [SerializeField] private GameObject luminanceCameraPrefab;
    [SerializeField] private ComputeShader luminanceSumShader;
    private int indexOfLuminanceSumShader;
    private List<IComputationObject> computationObjects;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(vegetationManager);
        Assert.IsNotNull(lineVisualizationControl);
        Assert.IsNotNull(gridVisualizationControl);
        Assert.IsNotNull(luminancePass);
        Assert.IsNotNull(luminanceCameraPrefab);
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

    public void ExportResults(List<Vector3> positions, List<float> luminances)
    {
        if (luminances.Count == 0) {
            DialogControl.CreateDialog("No results to export.");
        } else {
            string filename = SFB.StandaloneFileBrowser.SaveFilePanel("Export light results", "", "light_results", "geojson");
            if (filename != "") {
                List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

                for (int i=0; i<positions.Count; ++i) {
                    Vector3d coordinate = mapManager.GetCoordinatesFromUnityPosition(positions[i]);
                    GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(
                        coordinate.latitude,
                        coordinate.longitude,
                        coordinate.altitude
                    ));

                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("luminance", luminances[i]);

                    features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
                }

                GeoJSONParser.FeaturesToFile(filename, features);
            }
        }
    }

    private IEnumerator Compute(IComputationObject computationObject)
    {
        luminancePass.SetActive(true);

        RenderTexture luminanceTexture = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        luminanceTexture.enableRandomWrite = true;
        luminanceTexture.Create();

        Camera luminanceCamera = Instantiate(luminanceCameraPrefab, transform).GetComponent<Camera>();
        luminanceCamera.targetTexture = luminanceTexture;
        vegetationManager.AddCamera(luminanceCamera);

        Vector3[] positions;
        float[] angles;
        computationObject.GetPositionsAnglesAlongObject(out positions, out angles);
        float[] luminances = new float[positions.Length];

        // Skip frame to let luminancePass turning on
        yield return null;

        for (int i=0; i<positions.Length; ++i) {
            luminanceCamera.transform.position = positions[i];
            luminanceCamera.transform.eulerAngles = new Vector3(
                luminanceCamera.transform.eulerAngles.x,
                luminanceCamera.transform.eulerAngles.y,
                angles[i]
            );
        
            // Skip frame to render to camera
            yield return null;

            // Skip frame to render vegetation, needed only for the first point
            if (i ==0) {
                yield return null;
            }

            int[] result = new int[1];
            result[0] = 0;

            ComputeBuffer computeBuffer = new ComputeBuffer(1, 4);
            computeBuffer.SetData(result);

            luminanceSumShader.SetTexture(indexOfLuminanceSumShader, "Texture", luminanceTexture);
            luminanceSumShader.SetFloat("LuminanceResolution", LUMINANCE_RESOLUTION);
            luminanceSumShader.SetBuffer(indexOfLuminanceSumShader, "Result", computeBuffer);
            luminanceSumShader.Dispatch(indexOfLuminanceSumShader, luminanceTexture.width / 32, luminanceTexture.height / 32, 1);

            computeBuffer.GetData(result);
            luminances[i] = result[0] / LUMINANCE_RESOLUTION / (MASK_TEXTURE_SIZE * MASK_TEXTURE_SIZE);
            computeBuffer.Release();
        }

        vegetationManager.RemoveCamera(luminanceCamera);
        luminanceTexture.Release();
        Destroy(luminanceCamera.gameObject);
        luminancePass.SetActive(false);

        computationObject.ResultsComputed(positions, luminances);
    }
}