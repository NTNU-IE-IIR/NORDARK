using System.Collections.Generic;
using UnityEngine;
using BitMiracle.LibTiff.Classic;


public class TiffOpener : MonoBehaviour
{
    private const string HEIGHTMAPS_FOLDER = "heightmaps";
    private int numberOfPixelsX;
    private int numberOfPixelsY;
    private float noDataValue;
    private double startLat;
    private double endLat;
    private double startLon;
    private double endLon;

    void Start()
    {
        ProcessTiffFile(@"C:\Users\leole\Desktop\elevation-lerstad.tif");
    }

    private void ProcessTiffFile(string path)
    {
        using (Tiff tiff = Tiff.Open(path, "r")) {
            numberOfPixelsX = tiff.ScanlineSize() / 4;
            numberOfPixelsY = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            FieldValue[] modelPixelScaleTag = tiff.GetField((TiffTag)33550);
            FieldValue[] modelTiepointTag = tiff.GetField((TiffTag)33922);
            FieldValue[] noDataTag = tiff.GetField((TiffTag)42113);

            byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();
            double pixelSizeX = System.BitConverter.ToDouble(modelPixelScale, 0);
            double pixelSizeY = System.BitConverter.ToDouble(modelPixelScale, 8)*-1;

            byte[] modelTiepoint = modelTiepointTag[1].GetBytes();
            double originLon = System.BitConverter.ToDouble(modelTiepoint, 24);
            double originLat = System.BitConverter.ToDouble(modelTiepoint, 32);

            byte[] noData = noDataTag[1].GetBytes();
            noDataValue = float.Parse(System.Text.Encoding.ASCII.GetString(noData));

            endLat = originLat + (pixelSizeY / 2);
            startLat = endLat + numberOfPixelsY * pixelSizeY;
            startLon = originLon + (pixelSizeX / 2);
            endLon = startLon + numberOfPixelsX * pixelSizeX;

            CreateHeatmapTile(tiff);
        }
    }

    private void CreateHeatmapTile(Tiff tiff)
    {
        byte[] scanline = new byte[4*numberOfPixelsX];
        float[] scanline32Bit = new float[numberOfPixelsX];

        int startingIndexX = Mathf.Max(0, (int) (numberOfPixelsX * (6.2222 - startLon) / (endLon - startLon)));
        int endingIndexX = Mathf.Min(numberOfPixelsX, (int) (numberOfPixelsX * (6.3647 - startLon) / (endLon - startLon)));
        int startingIndexY = Mathf.Max(0, (int) (numberOfPixelsY * (62.4312- startLat) / (endLat - startLat)));
        int endingIndexY = Mathf.Min(numberOfPixelsY, (int) (numberOfPixelsY * (62.4907 - startLat) / (endLat - startLat)));

        if (endingIndexY > startingIndexY && endingIndexX > startingIndexX) {
            Texture2D texture = new Texture2D(endingIndexX - startingIndexX, endingIndexY - startingIndexY, TextureFormat.ARGB32, false);

            for (int j = startingIndexY; j < endingIndexY; ++j) {
                tiff.ReadScanline(scanline, j);
                System.Buffer.BlockCopy(scanline, 0, scanline32Bit, 0, scanline.Length);

                for (int i = startingIndexX; i<endingIndexX; ++i) {
                    double elevation = scanline32Bit[i] == noDataValue ? 0 : scanline32Bit[i];
                    
                    double elevationScaled = (elevation + 10000) * 10;
                    int red = (int) (elevationScaled / (256 * 256));
                    int green = (int) ((elevationScaled - red * (256 * 256)) / 256);
                    int blue = (int) ((elevationScaled - red * (256 * 256) - green * 256) / 1);
                    
                    texture.SetPixel(i + startingIndexX, endingIndexY - 1 - j + startingIndexY, new Color(
                        red / 255.0f,
                        green / 255.0f,
                        blue / 255.0f,
                        1
                    ));
                }                    
            }
            texture.Apply();
            texture = ResizeTexture(texture, 256, 256);

            System.IO.File.WriteAllBytes(CreateAndGetTilePath("test"), texture.EncodeToPNG());
        }
    }

    private Texture2D ResizeTexture(Texture2D texture, int targetX, int targetY)
    {
        RenderTexture old = RenderTexture.active;

        RenderTexture rt = new RenderTexture(targetX, targetY, 0);
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);

        Destroy(texture);
        texture = new Texture2D(targetX, targetY);
        texture.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        texture.Apply();

        RenderTexture.active = old;

        return texture;
    }

    private static string CreateAndGetTilePath(string tileId)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, HEIGHTMAPS_FOLDER);

        if (!System.IO.Directory.Exists(path)) {
            System.IO.Directory.CreateDirectory(path);
        }

        return System.IO.Path.Combine(path, tileId + ".png");
    }
}