Shader "FullScreen/LuminanceMapCustomPass"
{
    HLSLINCLUDE

    // Parameters set from script
    int numberOfColors;
    float4 colors[50];
    float values[50];

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        float luminance = Luminance(color) / GetCurrentExposureMultiplier();

        // Find highest i such that luminance < values[i]
        int i;
        if (values[0] < values[1]) {
            i = 0;
            while (i < numberOfColors-1 && luminance > values[i]) {
                i++;
            }
        } else {
            i = numberOfColors-1;
            while (i > 0 && luminance > values[i]) {
                i--;
            }
        }

        return colors[i];
    }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
