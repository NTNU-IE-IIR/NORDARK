Shader "FullScreen/LuminanceCustomPass"
{
    HLSLINCLUDE

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

        float lum = Luminance(color) / GetCurrentExposureMultiplier();

        int firstDecomposition = lum / 100.0;
        int secondDecomposition = (lum - firstDecomposition * 100) / 1.0;
        int thirdDecomposition = (lum - firstDecomposition * 100 - secondDecomposition * 1.0) / 0.01;

        return float4(
            firstDecomposition / 100.0,
            secondDecomposition / 100.0,
            thirdDecomposition / 100.0,
            1.0
        );
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
