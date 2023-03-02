using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IESSetter : MonoBehaviour
{
    private const string IES_RESOURCES_FOLDER = "IES";
    [SerializeField] private GameObject fullscreenPass;
    public bool lightComputation = true;
    private string IESDirectory;
    
    void Start()
    {
        IESDirectory = CreateAndGetDirectory();
        UnityEngine.Rendering.HighDefinition.HDAdditionalLightData hdAdditionalLightData = GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
        
        IESLight iesLight = GetIESLightFromName("Fox_Nyx 330_Comfort_830");
        hdAdditionalLightData.SetCookie(iesLight.Cookie);
        hdAdditionalLightData.SetIntensity(iesLight.Intensity.Value, iesLight.Intensity.Unit);
    }

    void Update()
    {
        DisplayLightResults(lightComputation);
    }

    IESLight GetIESLightFromName(string name)
    {
        List<IESLight> IESs = new List<IESLight>();
        foreach (string path in Directory.GetFiles(IESDirectory)) {
            string iesName = Path.GetFileNameWithoutExtension(path);

            if (iesName == name) {
                IESLight light = new IESLight(iesName, LoadCookie(path), GetIntensity(path));
                Debug.Log(path + " " + light.Cookie.GetPixel(CubemapFace.NegativeY, 35, 35));
                return light;
            }
        }

        return null;
    }

    private void DisplayLightResults(bool display)
    {
        fullscreenPass.SetActive(display);
    }

    private Cubemap LoadCookie(string path)
    {
        return IESLights.RuntimeIESImporter.ImportPointLightCookie(path, 128, false);
    }

    private LightIntensity GetIntensity(string path)
    {
        IESReader iesReader = new IESReader();
        iesReader.ReadFile(path);
        if (iesReader.TotalLumens != -1) {
            return new LightIntensity(iesReader.TotalLumens, UnityEngine.Rendering.HighDefinition.LightUnit.Lumen);
        } else {
            return new LightIntensity(iesReader.MaxCandelas, UnityEngine.Rendering.HighDefinition.LightUnit.Candela);
        }
    }

    private string CreateAndGetDirectory()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, IES_RESOURCES_FOLDER);
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }
}
