using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using SFB;

public class IESManager : MonoBehaviour
{
    private const string IES_RESOURCES_FOLDER = "IES";

    [SerializeField]
    private LightControl lightControl;
    [SerializeField]
    private DialogControl dialogControl;

    private List<IESLight> IESs;
    private string IESDirectory;

    void Awake()
    {
        Assert.IsNotNull(lightControl);
        Assert.IsNotNull(dialogControl);

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

    public void Upload()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Upload an IES file", "", "ies", true);
        List<string> addedFiles = new List<string>();
        List<string> notAddedFiles = new List<string>();

        foreach (string path in paths) {
            string iesName = Path.GetFileNameWithoutExtension(path);
            string destFile = Path.Combine(IESDirectory, Path.GetFileName(path));

            File.Copy(path, destFile, true);
            Cubemap cookie = LoadCookie(iesName);
            if (cookie != null) {
                IESs.Add(new IESLight(iesName, cookie, GetIntensity(iesName)));
                lightControl.SetIESNames(GetIESNames());
                addedFiles.Add(iesName);
            } else {
                File.Delete(destFile);
                notAddedFiles.Add(iesName);
            }
        }

        string message = "";
        if (addedFiles.Count > 0) {
            message += "The following IES files were added:\n";
            foreach (string file in addedFiles) {
                message += file + "\n";
            }
        }
        if (notAddedFiles.Count > 0) {
            message += "The following IES files were not added:\n";
            foreach (string file in notAddedFiles) {
                message += file + "\n";
            }
        }
        if (message != "") {
            dialogControl.CreateInfoDialog(message);
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
