using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IESSetter : MonoBehaviour
{
    public bool texture2D = false;
    UnityEngine.Rendering.HighDefinition.HDAdditionalLightData hdAdditionalLightData;

    void Start()
    {
        hdAdditionalLightData = GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();

        if (texture2D) {
            Texture2D cookie = IESLights.RuntimeIESImporter.ImportSpotlightCookie(@"D:\Research_Assistant\Nordark\Project\NORDARK\Assets\Temp\BE_99527K3.ies");
            hdAdditionalLightData.SetCookie(cookie);
        } else {
            Cubemap cookie = IESLights.RuntimeIESImporter.ImportPointLightCookie(@"D:\Research_Assistant\Nordark\Project\NORDARK\Assets\Temp\BE_99527K3.ies");
            hdAdditionalLightData.SetCookie(cookie);
        }
    }
}
