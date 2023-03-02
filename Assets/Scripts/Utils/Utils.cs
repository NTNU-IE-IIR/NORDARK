using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Utils
{
    public static string DetermineNewName(List<string> names, string baseName, int index = 1)
    {
        foreach (string name in names) {
            if (name == baseName + " " + index.ToString()) {
                return DetermineNewName(names, baseName, index + 1);
            }
        }
        return baseName + " " + index.ToString();
    }

    public static float GetAngleBetweenPositions(Vector2 positionA, Vector2 positionB)
    {
        return Mathf.Rad2Deg * Mathf.Asin((positionB.y - positionA.y) / Vector3.Distance(positionA, positionB));
    }

    public static bool IsEPSG4326(GeoJSON.Net.Geometry.IPosition coordinates)
    {
        return coordinates.Longitude > -180 && coordinates.Longitude < 180 && coordinates.Latitude > -90 && coordinates.Longitude < 90;
    }

    public static float GetLineDistance(LineRenderer line)
    {
        float distance = 0;
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        for (int i=0; i<line.positionCount-1; ++i) {
            distance += Vector3.Distance(positions[i], positions[i+1]);
        }
        return distance;
    }

    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles()) {
            if (file.Extension != ".meta") {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }
        }

        if (recursive) {
            foreach (DirectoryInfo subDir in dirs) {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
