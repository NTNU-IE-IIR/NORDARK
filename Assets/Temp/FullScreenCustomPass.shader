Shader "FullScreen/FullScreenCustomPass"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 CustomPassSampleCustomColor(float2 uv);
    // float4 CustomPassLoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float4 color = float4(0.0, 0.0, 0.0, 0.0);

        // Load the camera color buffer at the mip 0 if we're not at the before rendering injection point
        if (_CustomPassInjectionPoint != CUSTOMPASSINJECTIONPOINT_BEFORE_RENDERING)
            color = float4(CustomPassLoadCameraColor(varyings.positionCS.xy, 0), 1);

        // Add your custom pass code here
        float lum = Luminance(color) / GetCurrentExposureMultiplier();
        if (lum > 15000) {
            color = float4(0.89, 0.89, 0.89, 1.0);
        } else if (lum > 10000) {
            color = float4(0.996, 0.580, 0.6, 1.0);
        } else if (lum > 7500) {
            color = float4(0.996, 0.337, 0.267, 1.0);
        } else if (lum > 5000) {
            color = float4(0.996, 0.0, 0.0, 1.0);
        } else if (lum > 3000) {
            color = float4(0.996, 0.537, 0.0, 1.0);
        } else if (lum > 2000) {
            color = float4(0.996, 0.702, 0.11, 1.0);
        } else if (lum > 1000) {
            color = float4(0.996, 0.863, 0.541, 1.0);
        } else if (lum > 750) {
            color = float4(0.996, 0.996, 0.0, 1.0);
        } else if (lum > 500) {
            color = float4(0.871, 0.996, 0.322, 1.0);
        } else if (lum > 300) {
            color = float4(0.322, 0.871, 0.463, 1.0);
        } else if (lum > 200) {
            color = float4(0.451, 0.859, 0.0, 1.0);
        } else if (lum > 100) {
            color = float4(0.004, 0.996, 0.0, 1.0);
        } else if (lum > 75) {
            color = float4(0.082, 0.996, 0.671, 1.0);
        } else if (lum > 50) {
            color = float4(0.0, 0.769, 0.659, 1.0);
        } else if (lum > 30) {
            color = float4(0.004, 0.996, 0.996, 1.0);
        } else if (lum > 20) {
            color = float4(0.447, 0.6663, 0.996, 1.0);
        } else if (lum > 10) {
            color = float4(0.204, 0.561, 0.996, 1.0);
        } else if (lum > 7.5) {
            color = float4(0.0, 0.259, 0.996, 1.0);
        } else if (lum > 5) {
            color = float4(0.435, 0.376, 0.996, 1.0);
        } else if (lum > 3) {
            color = float4(0.6, 0.180, 0.62, 1.0);
        } else if (lum > 2) {
            color = float4(0.529, 0.239, 0.996, 1.0);
        } else if (lum > 1) {
            color = float4(0.490, 0.0, 0.522, 1.0);
        } else if (lum > 0.75) {
            color = float4(0.404, 0.0, 0.227, 1.0);
        } else if (lum > 0.50) {
            color = float4(0.239, 0.0, 0.247, 1.0);
        } else if (lum > 0.30) {
            color = float4(0.0, 0.0, 0.239, 1.0);
        } else if (lum > 0.20) {
            color = float4(0.11, 0.0, 0.149, 1.0);
        } else {
            color = float4(0.0, 0.0, 0.0, 1.0);
        }
        
        return color;
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
