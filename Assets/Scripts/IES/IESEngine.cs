using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class IESEngine
{
    const float k_HalfPi = 0.5f * Mathf.PI;
    const float k_TwoPi = 2.0f * Mathf.PI;
    internal IESReader m_iesReader = new IESReader();
    internal string FileFormatVersion { get => m_iesReader.FileFormatVersion; }

    internal TextureImporterType m_TextureGenerationType = TextureImporterType.Cookie;
    public TextureImporterType TextureGenerationType
    {
        set { m_TextureGenerationType = value; }
    }

    public string GetPhotometricType()
    {
        switch (m_iesReader.PhotometricType)
        {
            case 3: // type A
                return "Type A";
            case 2: // type B
                return "Type B";
            default: // type C
                return "Type C";
        }
    }
    public string GetKeywordValue(string keyword)
    {
        return m_iesReader.GetKeywordValue(keyword);
    }

    public string ReadFile(string iesFilePath)
    {
        if (!File.Exists(iesFilePath))
        {
            return "IES file does not exist.";
        }

        string errorMessage;

        try
        {
            errorMessage = m_iesReader.ReadFile(iesFilePath);
        }
        catch (IOException ioEx)
        {
            return ioEx.Message;
        }

        return errorMessage;
    }

    public (float, string) GetMaximumIntensity()
    {
        if (m_iesReader.TotalLumens == -1f) // absolute photometry
        {
            return (m_iesReader.MaxCandelas, "Candelas");
        }
        else
        {
            return (m_iesReader.TotalLumens, "Lumens");
        }
    }

    public (string, Texture) Generate2DCookie(TextureImporterCompression compression, float coneAngle, int textureSize, bool applyLightAttenuation)
    {
        NativeArray<Color32> colorBuffer;

        switch (m_iesReader.PhotometricType)
        {
            case 3: // type A
                colorBuffer = BuildTypeAGnomonicTexture(coneAngle, textureSize, applyLightAttenuation);
                break;
            case 2: // type B
                colorBuffer = BuildTypeBGnomonicTexture(coneAngle, textureSize, applyLightAttenuation);
                break;
            default: // type C
                colorBuffer = BuildTypeCGnomonicTexture(coneAngle, textureSize, applyLightAttenuation);
                break;
        }

        return GenerateTexture(m_TextureGenerationType, TextureImporterShape.Texture2D, compression, textureSize, textureSize, colorBuffer);
    }

    NativeArray<Color32> BuildTypeAGnomonicTexture(float coneAngle, int size, bool applyLightAttenuation)
    {
        float limitUV = Mathf.Tan(0.5f * coneAngle * Mathf.Deg2Rad);
        float stepUV = (2 * limitUV) / (size - 3);

        var textureBuffer = new NativeArray<Color32>(size * size, Allocator.Temp, NativeArrayOptions.ClearMemory);

        // Leave a one-pixel black border around the texture to avoid cookie spilling.
        for (int y = 1; y < size - 1; y++)
        {
            var slice = new NativeSlice<Color32>(textureBuffer, y * size, size);

            float v = (y - 1) * stepUV - limitUV;

            for (int x = 1; x < size - 1; x++)
            {
                float u = (x - 1) * stepUV - limitUV;

                float rayLengthSquared = u * u + v * v + 1;

                float longitude = Mathf.Atan(u) * Mathf.Rad2Deg;                               // in range [-90..+90] degrees
                float latitude = Mathf.Asin(v / Mathf.Sqrt(rayLengthSquared)) * Mathf.Rad2Deg; // in range [-90..+90] degrees

                float horizontalAnglePosition = m_iesReader.ComputeTypeCHorizontalAnglePosition(longitude);
                float verticalAnglePosition = m_iesReader.ComputeVerticalAnglePosition(latitude);

                // Factor in the light attenuation further from the texture center.
                float lightAttenuation = applyLightAttenuation ? rayLengthSquared : 1f;

                byte value = (byte)((m_iesReader.InterpolateBilinear(horizontalAnglePosition, verticalAnglePosition) / (m_iesReader.MaxCandelas * lightAttenuation)) * 255);
                slice[x] = new Color32(value, value, value, value);
            }
        }

        return textureBuffer;
    }

    NativeArray<Color32> BuildTypeBGnomonicTexture(float coneAngle, int size, bool applyLightAttenuation)
    {
        float limitUV = Mathf.Tan(0.5f * coneAngle * Mathf.Deg2Rad);
        float stepUV = (2 * limitUV) / (size - 3);

        var textureBuffer = new NativeArray<Color32>(size * size, Allocator.Temp, NativeArrayOptions.ClearMemory);

        // Leave a one-pixel black border around the texture to avoid cookie spilling.
        for (int y = 1; y < size - 1; y++)
        {
            var slice = new NativeSlice<Color32>(textureBuffer, y * size, size);

            float v = (y - 1) * stepUV - limitUV;

            for (int x = 1; x < size - 1; x++)
            {
                float u = (x - 1) * stepUV - limitUV;

                float rayLengthSquared = u * u + v * v + 1;

                // Since a type B luminaire is turned on its side, U and V are flipped.
                float longitude = Mathf.Atan(v) * Mathf.Rad2Deg;                               // in range [-90..+90] degrees
                float latitude = Mathf.Asin(u / Mathf.Sqrt(rayLengthSquared)) * Mathf.Rad2Deg; // in range [-90..+90] degrees

                float horizontalAnglePosition = m_iesReader.ComputeTypeCHorizontalAnglePosition(longitude);
                float verticalAnglePosition = m_iesReader.ComputeVerticalAnglePosition(latitude);

                // Factor in the light attenuation further from the texture center.
                float lightAttenuation = applyLightAttenuation ? rayLengthSquared : 1f;

                byte value = (byte)((m_iesReader.InterpolateBilinear(horizontalAnglePosition, verticalAnglePosition) / (m_iesReader.MaxCandelas * lightAttenuation)) * 255);
                slice[x] = new Color32(value, value, value, value);
            }
        }

        return textureBuffer;
    }

    NativeArray<Color32> BuildTypeCGnomonicTexture(float coneAngle, int size, bool applyLightAttenuation)
    {
        float limitUV = Mathf.Tan(0.5f * coneAngle * Mathf.Deg2Rad);
        float stepUV = (2 * limitUV) / (size - 3);

        var textureBuffer = new NativeArray<Color32>(size * size, Allocator.Temp, NativeArrayOptions.ClearMemory);

        // Leave a one-pixel black border around the texture to avoid cookie spilling.
        for (int y = 1; y < size - 1; y++)
        {
            var slice = new NativeSlice<Color32>(textureBuffer, y * size, size);

            float v = (y - 1) * stepUV - limitUV;

            for (int x = 1; x < size - 1; x++)
            {
                float u = (x - 1) * stepUV - limitUV;

                float uvLength = Mathf.Sqrt(u * u + v * v);

                float longitude = ((Mathf.Atan2(v, u) - k_HalfPi + k_TwoPi) % k_TwoPi) * Mathf.Rad2Deg; // in range [0..360] degrees
                float latitude = Mathf.Atan(uvLength) * Mathf.Rad2Deg;                                  // in range [0..90] degrees

                float horizontalAnglePosition = m_iesReader.ComputeTypeCHorizontalAnglePosition(longitude);
                float verticalAnglePosition = m_iesReader.ComputeVerticalAnglePosition(latitude);

                // Factor in the light attenuation further from the texture center.
                float lightAttenuation = applyLightAttenuation ? (uvLength * uvLength + 1) : 1f;

                byte value = (byte)((m_iesReader.InterpolateBilinear(horizontalAnglePosition, verticalAnglePosition) / (m_iesReader.MaxCandelas * lightAttenuation)) * 255);
                slice[x] = new Color32(value, value, value, value);
            }
        }

        return textureBuffer;
    }

    (string, Texture) GenerateTexture(TextureImporterType type, TextureImporterShape shape, TextureImporterCompression compression, int width, int height, NativeArray<Color32> colorBuffer)
    {
        TextureCreationFlags flags = TextureCreationFlags.None;
        UnityEngine.Texture2D texture = new Texture2D(width, height, GraphicsFormat.RGBA_BC7_UNorm, flags);
        texture.SetPixelData(colorBuffer, 0);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.anisoLevel = 0;
        texture.Apply();
        return ("", texture);

        /*
        // Default values set by the TextureGenerationSettings constructor can be found in this file on GitHub:
        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/AssetPipeline/TextureGenerator.bindings.cs

        var settings = new TextureGenerationSettings(type);

        SourceTextureInformation textureInfo = settings.sourceTextureInformation;
        textureInfo.containsAlpha = true;
        textureInfo.height = height;
        textureInfo.width = width;

        TextureImporterSettings textureImporterSettings = settings.textureImporterSettings;
        textureImporterSettings.alphaSource = TextureImporterAlphaSource.FromInput;
        textureImporterSettings.aniso = 0;
        textureImporterSettings.borderMipmap = (textureImporterSettings.textureType == TextureImporterType.Cookie);
        textureImporterSettings.filterMode = FilterMode.Bilinear;
        textureImporterSettings.generateCubemap = TextureImporterGenerateCubemap.Cylindrical;
        textureImporterSettings.mipmapEnabled = false;
        textureImporterSettings.npotScale = TextureImporterNPOTScale.None;
        textureImporterSettings.readable = true;
        textureImporterSettings.sRGBTexture = false;
        textureImporterSettings.textureShape = shape;
        textureImporterSettings.wrapMode = textureImporterSettings.wrapModeU = textureImporterSettings.wrapModeV = textureImporterSettings.wrapModeW = TextureWrapMode.Clamp;

        TextureImporterPlatformSettings platformSettings = settings.platformSettings;
        platformSettings.maxTextureSize = 2048;
        platformSettings.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
        platformSettings.textureCompression = compression;

        TextureGenerationOutput output = TextureGenerator.GenerateTexture(settings, colorBuffer);

        if (output.importWarnings.Length > 0)
        {
            Debug.LogWarning("Cannot properly generate IES texture:\n" + string.Join("\n", output.importWarnings));
        }

        return (output.importInspectorWarnings, output.texture);
        */
    }
}
