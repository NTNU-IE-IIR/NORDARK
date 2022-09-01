using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class IESManager : MonoBehaviour
{
    private const string IES_RESOURCES_FOLDER = "IES";

    [SerializeField]
    private LightControl lightControl;

    private List<IESLight> IESs;
    private string IESDirectory;

    void Awake()
    {
        Assert.IsNotNull(lightControl);

        IESDirectory = CreateAndGetDirectory();
        
        IESs = new List<IESLight>();
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
        lightControl.SetIESNames(GetIESNames());
    }

    public void AddIES(string path)
    {
        string iesName = Path.GetFileNameWithoutExtension(path);
        string destFile = Path.Combine(IESDirectory, Path.GetFileName(path));

        File.Copy(path, destFile, true);
        Texture2D cookie = LoadCookie(iesName);
        if (cookie != null) {
            IESs.Add(new IESLight(iesName, cookie, GetIntensity(iesName)));
            lightControl.SetIESNames(GetIESNames());
        } else {
            File.Delete(destFile);
            Debug.Log("The IES file could not be read");
        }
    }

    public IESLight GetIESLightFromName(string name)
    {
        IESLight IES = IESs.Find(iesLight => iesLight.Name == name);
        if (IES == null) {
            IES =  IESs[0];
        }
        return IES;
    }

    private string CreateAndGetDirectory()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, IES_RESOURCES_FOLDER);
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private Texture2D LoadCookie(string iesName)
    {
        string path = GetPathFromFileNameAndCreateFile(iesName);
        return IESLights.RuntimeIESImporter.ImportSpotlightCookie(path, 128, false, false);
    }

    private float GetIntensity(string iesName)
    {
        string path = GetPathFromFileNameAndCreateFile(iesName);

        IESReader iesReader = new IESReader();
        iesReader.ReadFile(path);
        return iesReader.MaxCandelas;
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

    private void AddText(FileStream file, string value)
    {
        byte[] info = new System.Text.UTF8Encoding(true).GetBytes(value);
        file.Write(info, 0, info.Length);
    }

    private List<string> GetIESNames()
    {
        return IESs.Select(IESLight => IESLight.Name).ToList();
    }
}
