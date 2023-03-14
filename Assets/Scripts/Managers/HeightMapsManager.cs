using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using BitMiracle.LibTiff.Classic;

public class HeightMapsManager : MonoBehaviour
{
    private const string HEIGHTMAPS_FOLDER = "heightmaps";
    [SerializeField] private MapManager mapManager;
    private int numberOfPixelsX;
    private int numberOfPixelsY;
    private float noDataValue;
    private double startLat;
    private double endLat;
    private double startLon;
    private double endLon;

    void Awake()
    {
        Assert.IsNotNull(mapManager);
    }

    public void AddHeightMapsFromResources()
    {
        Utils.AddFolderFromResources(HEIGHTMAPS_FOLDER);
    }

    public void AddHeightMapFromFile()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel(
            "Import height maps into the scene",
            "",
            new SFB.ExtensionFilter[] { new SFB.ExtensionFilter("tiff", "tif") },
            true
        );

        List<string> addedFiles = new List<string>();
        List<string> nonAddedFiles = new List<string>();

        foreach (string path in paths) {
            try {
                if (ProcessTiffFile(path)) {
                    addedFiles.Add(System.IO.Path.GetFileName(path));
                } else {
                    nonAddedFiles.Add(System.IO.Path.GetFileName(path));
                }
            } catch (System.Exception e) {
                DialogControl.CreateDialog(e.Message);
            }
        }

        mapManager.UpdateMapTerrain();

        string message = "";
        if (addedFiles.Count == paths.Length && paths.Length > 0) {
            message += "All files added.";
        } else {
            if (addedFiles.Count > 0) {
                message += "The following files were added:\n";
                foreach (string addedFile in addedFiles) {
                    message += addedFile + ", ";
                }
                message = message.Remove(message.Length - 2, 2);
                message += ".\n";
            }

            if (nonAddedFiles.Count > 0) {
                message += "The following files were not added:\n";
                foreach (string nonAddedFile in nonAddedFiles) {
                    message += nonAddedFile + ", ";
                }
                message = message.Remove(message.Length - 2, 2);
                message += ".\nMake sure these files use the EPSG:4326 - WGS 84 coordinate system and that the correct location is selected.";
            }
        }

        if (message != "") {
            DialogControl.CreateDialog(message);
        }
    }

    public static byte[] GetElevationImage(string tileId)
    {
        byte[] elevation = null;

        string path = CreateAndGetTilePath(tileId.Replace('/', '-'));
        if (System.IO.File.Exists(path)) {
            elevation = System.IO.File.ReadAllBytes(path);
        }

        return elevation;
    }

    private bool ProcessTiffFile(string path)
    {
        bool atLeastOneTextureAdded = false;

        using (Tiff tiff = Tiff.Open(path, "r")) {
            numberOfPixelsX = tiff.ScanlineSize() / 4;
            numberOfPixelsY = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            // The tag values/types come from the GeoTIFF specifications
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

            List<Tile> tiles = mapManager.GetTiles();
            foreach (Tile tile in tiles) {
                atLeastOneTextureAdded = CreateHeatmapTile(tiff, tile) || atLeastOneTextureAdded;
            }
        }

        return atLeastOneTextureAdded;
    }

    private bool CreateHeatmapTile(Tiff tiff, Tile tile)
    {
        // Each geotiff pixel is represented by a 4 bytes float, but the libtiff library returns an array of one byte values.
        // So a conversion has to be done
        byte[] scanline = new byte[4*numberOfPixelsX];
        float[] scanline32Bit = new float[numberOfPixelsX];

        (Coordinate, Coordinate) boundaries = mapManager.GetTileBoundaries(tile);

        int startingIndexX = Mathf.Max(0, (int) (numberOfPixelsX * (boundaries.Item1.longitude - startLon) / (endLon - startLon)));
        int endingIndexX = Mathf.Min(numberOfPixelsX, (int) (numberOfPixelsX * (boundaries.Item2.longitude - startLon) / (endLon - startLon)));
        int startingIndexY = numberOfPixelsY - Mathf.Min(numberOfPixelsY, (int) (numberOfPixelsY * (boundaries.Item2.latitude - startLat) / (endLat - startLat)));
        int endingIndexY = numberOfPixelsY - Mathf.Max(0, (int) (numberOfPixelsY * (boundaries.Item1.latitude - startLat) / (endLat - startLat)));
        
        if (endingIndexY > startingIndexY && endingIndexX > startingIndexX) {
            Texture2D texture = new Texture2D(endingIndexX - startingIndexX, endingIndexY - startingIndexY, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            for (int j = startingIndexY; j<endingIndexY; ++j) {
                tiff.ReadScanline(scanline, j);
                System.Buffer.BlockCopy(scanline, 0, scanline32Bit, 0, scanline.Length);

                for (int i = startingIndexX; i<endingIndexX; ++i) {
                    double elevation = scanline32Bit[i] == noDataValue ? 0 : scanline32Bit[i];
                    
                    // See https://docs.mapbox.com/data/tilesets/guides/access-elevation-data/#decode-data
                    double elevationScaled = (elevation + 10000) * 10 ;
                    int red = (int) (elevationScaled / (256 * 256));
                    int green = (int) ((elevationScaled - red * (256 * 256)) / 256);
                    int blue = (int) ((elevationScaled - red * (256 * 256) - green * 256) / 1);
                    
                    texture.SetPixel(i - startingIndexX, endingIndexY - 1 - j, new Color(
                        red / 255.0f,
                        green / 255.0f,
                        blue / 255.0f,
                        1
                    ));
                }                    
            }
            texture.Apply();
            texture = ResizeTexture(texture, 256, 256);
            System.IO.File.WriteAllBytes(CreateAndGetTilePath(tile.Id), texture.EncodeToPNG());
            Destroy(texture);
            
            return true;
        } else {
            return false;
        }
    }

    private Texture2D ResizeTexture(Texture2D texture, int targetX, int targetY)
    {
        RenderTexture old = RenderTexture.active;

        RenderTexture renderTexture = RenderTexture.GetTemporary(targetX, targetY, 0);
        RenderTexture.active = renderTexture;
        Graphics.Blit(texture, renderTexture);

        Destroy(texture);
        texture = new Texture2D(targetX, targetY);
        texture.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        texture.Apply();

        RenderTexture.active = old;

        RenderTexture.ReleaseTemporary(renderTexture);

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