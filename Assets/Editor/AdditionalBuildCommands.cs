using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AdditionalBuildCommands : UnityEditor.Build.IPostprocessBuildWithReport
{
    public int callbackOrder { get; set; }

    public void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        List<string> filesToCopy = GetFilesToCopy();
        List<string> directoriesToCopy = GetDirectoriesToCopy();
        string resourceFolder = CreateAndGetResourceFolder(report);

        foreach (string fileToCopy in filesToCopy) {
            File.Copy(fileToCopy, Path.Combine(resourceFolder, Path.GetFileName(fileToCopy)), true);
        }

        foreach (string directoryToCopy in directoriesToCopy) {
            Utils.CopyDirectory(directoryToCopy, Path.Combine(resourceFolder, Path.GetFileName(directoryToCopy)), true);
        }
    }

    private List<string> GetFilesToCopy()
    {
        List<string> filesToCopy = new List<string>();
        string[] filePaths = Directory.GetFiles(Path.Combine(Application.dataPath, GameManager.RESOURCES_FOLDER_NAME));
        foreach (string file in filePaths) {
            if (Path.GetExtension(file) != ".meta") {
                filesToCopy.Add(file);
            }
        }
        return filesToCopy;
    }

    private List<string> GetDirectoriesToCopy()
    {
        return Directory.GetDirectories(Path.Combine(Application.dataPath, GameManager.RESOURCES_FOLDER_NAME)).ToList();
    }

    private string CreateAndGetResourceFolder(UnityEditor.Build.Reporting.BuildReport report)
    {
        string folder = report.summary.platform == UnityEditor.BuildTarget.StandaloneOSX ?
            Path.Combine(Path.GetDirectoryName(report.summary.outputPath), UnityEditor.PlayerSettings.productName + ".app", "Contents", GameManager.RESOURCES_FOLDER_NAME) :
            Path.Combine(Path.ChangeExtension(report.summary.outputPath, null) + "_Data", GameManager.RESOURCES_FOLDER_NAME)
        ;
        Directory.CreateDirectory(folder);
        return folder;
    }
}