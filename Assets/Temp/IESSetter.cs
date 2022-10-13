using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IESSetter : MonoBehaviour
{
    public bool texture2D = false;
    private string file = "BE_99527K3";
    UnityEngine.Rendering.HighDefinition.HDAdditionalLightData hdAdditionalLightData;

    void Start()
    {
        hdAdditionalLightData = GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();

        if (texture2D) {
            Texture2D cookie = IESLights.RuntimeIESImporter.ImportSpotlightCookie(@"C:\Users\leole\Documents\NORDARK\Assets\Temp\" + file + ".ies");
            hdAdditionalLightData.SetCookie(cookie);
        } else {
            Cubemap cookie = IESLights.RuntimeIESImporter.ImportPointLightCookie(@"C:\Users\leole\Documents\NORDARK\Assets\Temp\" + file + ".ies");
            hdAdditionalLightData.SetCookie(cookie);
        }
    }
}
