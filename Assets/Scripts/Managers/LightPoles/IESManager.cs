using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class IESManager : MonoBehaviour
{
    private const string IES_RESOURCES_FOLDER = "IES";
    [SerializeField] private LightPolesControl lightPolesControl;
    private List<IESLight> IESs;
    private string IESDirectory;

    void Awake()
    {
        Assert.IsNotNull(lightPolesControl);
   
        IESDirectory = CreateAndGetDirectory();
    }

    public void AddIESFilesFromResources()
    {
        Utils.AddFolderFromResources(IES_RESOURCES_FOLDER);
    }

    public void Upload()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Upload an IES file", "", "ies", true);
        List<string> addedFiles = new List<string>();
        List<string> notAddedFiles = new List<string>();

        foreach (string path in paths) {
            string iesName = Path.GetFileNameWithoutExtension(path);
            string destFile = Path.Combine(IESDirectory, Path.GetFileName(path));

            File.Copy(path, destFile, true);
            Cubemap cookie = LoadCookie(destFile);
            if (cookie != null) {
                IESs.Add(new IESLight(iesName, cookie, GetIntensity(destFile)));
                lightPolesControl.SetIESNames(GetIESNames());
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
            DialogControl.CreateDialog(message);
        }
    }

    public IESLight GetIESLightFromName(string name)
    {
        if (IESs == null) {
            SetIESFiles();
        }

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

    private void SetIESFiles()
    {
        IESs = new List<IESLight>();
        foreach (string path in Directory.GetFiles(IESDirectory)) {
            IESs.Add(new IESLight(Path.GetFileNameWithoutExtension(path), LoadCookie(path), GetIntensity(path)));
        }
        lightPolesControl.SetIESNames(GetIESNames());
    }

    private List<string> GetIESNames()
    {
        return IESs.Select(IESLight => IESLight.Name).ToList();
    }

    private Cubemap LoadCookie(string path)
    {
        return IESLights.RuntimeIESImporter.ImportPointLightCookie(path, 128, false);
    }

    private LightIntensity GetIntensity(string path)
    {
        IESReader iesReader = new IESReader();
        iesReader.ReadFile(path);

        // Sometimes Lumens are not specified in the IES file, so we use Candela in such case
        if (iesReader.TotalLumens == -1) {
            return new LightIntensity(iesReader.MaxCandelas, UnityEngine.Rendering.HighDefinition.LightUnit.Candela);
        } else {
            return new LightIntensity(iesReader.TotalLumens, UnityEngine.Rendering.HighDefinition.LightUnit.Lumen);
        }
    }
}