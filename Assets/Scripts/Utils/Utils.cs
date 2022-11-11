using System.Collections.Generic;

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
}
