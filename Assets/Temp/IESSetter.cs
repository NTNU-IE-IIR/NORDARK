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
        Object[] IES = Resources.LoadAll(IES_RESOURCES_FOLDER);
        foreach (Object ies in IES) {
            IESs.Add(new IESLight(ies.name, LoadCookie(ies.name), GetIntensity(ies.name)));
        }
        foreach (string path in Directory.GetFiles(IESDirectory)) {
            string iesName = Path.GetFileNameWithoutExtension(path);
            if (!IESs.Any(iesLight => iesLight.Name.Equals(iesName))) {
                IESs.Add(new IESLight(iesName, LoadCookie(iesName), GetIntensity(iesName)));
            }
        }

        IESLight IESLight = IESs.Find(iesLight => iesLight.Name == name);
        if (IESLight == null) {
            IESLight =  IESs[0];
        }
        return IESLight;
    }

    private void DisplayLightResults(bool display)
    {
        fullscreenPass.SetActive(display);
    }

    private Cubemap LoadCookie(string iesName)
    {
        string path = GetPathFromFileNameAndCreateFile(iesName);
        return IESLights.RuntimeIESImporter.ImportPointLightCookie(path, 128, false);
    }

    private LightIntensity GetIntensity(string iesName)
    {
        string path = GetPathFromFileNameAndCreateFile(iesName);

        IESReader iesReader = new IESReader();
        iesReader.ReadFile(path);
        if (iesReader.TotalLumens != -1) {
            return new LightIntensity(iesReader.TotalLumens, UnityEngine.Rendering.HighDefinition.LightUnit.Lumen);
        } else {
            return new LightIntensity(iesReader.MaxCandelas, UnityEngine.Rendering.HighDefinition.LightUnit.Candela);
        }
    }

    private string GetPathFromFileNameAndCreateFile(string name)
    {
        string path = Path.Combine(IESDirectory, name + ".ies");

        if (!File.Exists(path)) {
            TextAsset iesResource = UnityEngine.Resources.Load<TextAsset>(IES_RESOURCES_FOLDER + "/" + name);
            using (FileStream file = File.Create(path))
            {
                AddText(file, iesResource.text);
            }
        }
        return path;
    }

    private string CreateAndGetDirectory()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, IES_RESOURCES_FOLDER);
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private void AddText(FileStream file, string value)
    {
        byte[] info = new System.Text.UTF8Encoding(true).GetBytes(value);
        file.Write(info, 0, info.Length);
    }
}
