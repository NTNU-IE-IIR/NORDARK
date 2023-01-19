using System.Collections.Generic;
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
}
