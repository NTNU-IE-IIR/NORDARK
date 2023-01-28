using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LightComputationManager : MonoBehaviour
{
    private const int MASK_TEXTURE_SIZE = 32;
    private const float LUMINANCE_RESOLUTION = 100;
    [SerializeField] private ObjectVisualizationControl lineVisualizationControl;
    [SerializeField] private ObjectVisualizationControl gridVisualizationControl;
    [SerializeField] private GameObject luminancePass;
    [SerializeField] private GameObject luminanceCameraPrefab;
    [SerializeField] private ComputeShader luminanceSumShader;
    private int indexOfLuminanceSumShader;
    private List<IComputationObject> computationObjects;

    void Awake()
    {
        Assert.IsNotNull(lineVisualizationControl);
        Assert.IsNotNull(gridVisualizationControl);
        Assert.IsNotNull(luminancePass);
        Assert.IsNotNull(luminanceCameraPrefab);
        Assert.IsNotNull(luminanceSumShader);

        indexOfLuminanceSumShader = luminanceSumShader.FindKernel("CSMain");
        computationObjects = new List<IComputationObject> {
            lineVisualizationControl.GetComputationObject(),
            //gridVisualizationControl.GetComputationObject()
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

    public void ObjectDefined(IComputationObject computationObject)
    {
        StartCoroutine(ComputeAlongObject(computationObject));
    }

    private IEnumerator ComputeAlongObject(IComputationObject computationObject)
    {
        luminancePass.SetActive(true);

        RenderTexture luminanceTexture = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        luminanceTexture.enableRandomWrite = true;
        luminanceTexture.Create();

        Camera luminanceCamera = Instantiate(luminanceCameraPrefab, transform).GetComponent<Camera>();
        luminanceCamera.targetTexture = luminanceTexture;

        Vector3[] positions;
        float[] angles;
        computationObject.GetPositionsAnglesAlongObject(out positions, out angles);
        
        // Skip frame to let luminancePass turning on
        yield return null;

        float[] luminances = new float[positions.Length];

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

            luminanceSumShader.SetTexture(indexOfLuminanceSumShader, "Texture", luminanceTexture);
            luminanceSumShader.SetFloat("LuminanceResolution", LUMINANCE_RESOLUTION);
            luminanceSumShader.SetBuffer(indexOfLuminanceSumShader, "Result", computeBuffer);
            luminanceSumShader.Dispatch(indexOfLuminanceSumShader, luminanceTexture.width / 32, luminanceTexture.height / 32, 1);

            computeBuffer.GetData(result);
            luminances[i] = result[0] / LUMINANCE_RESOLUTION / (MASK_TEXTURE_SIZE * MASK_TEXTURE_SIZE);

            computeBuffer.Release();
        }

        luminanceTexture.Release();
        Destroy(luminanceCamera.gameObject);
        luminancePass.SetActive(false);

        computationObject.ResultsComputed(positions, luminances);
    }
}