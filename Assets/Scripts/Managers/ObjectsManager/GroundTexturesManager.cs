using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GroundTexturesManager : ObjectsManager
{
    private const string TILING_BASE_PROPERTY = "_TilingBase";
    private const string DIFFUSE_BASE_PROPERTY = "_MainTex";
    private const string NORMAL_MAP_BASE_PROPERTY = "_NormalMapBase";
    private const string MASK_MAP_BASE_PROPERTY = "_MaskMapBase";
    private const string DIFFUSE_PROPERTY = "_Diffuse";
    private const string NORMAL_MAP_PROPERTY = "_NormalMap";
    private const string MASK_MAP_PROPERTY = "_MaskMap";
    private const string MASK_PROPERTY = "_Mask";
    private const string TEXTURE_PROPERTY = "_Texture";
    private const int MAX_NUMBER_OF_TEXTURES_PER_MATERIAL = 8;
    private const float GROUND_TILING = 100;
    [SerializeField] private GroundTextureMasksManager groundTextureMasksManager;
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private Texture2D baseMaskMap;
    [SerializeField] private List<string> textureNames;
    [SerializeField] private List<Texture2D> diffuseMaps;
    [SerializeField] private List<Texture2D> normalMaps;
    [SerializeField] private List<Texture2D> maskMaps;
    [SerializeField] private ComputeShader detectBlackMaskShader;
    private List<GroundTextureCollection> groundTextureCollections;
    private int indexOfDetectBlackMaskKernel;
    
    void Awake()
    {
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(groundTextureMasksManager);
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(baseMaskMap);
        Assert.IsNotNull(textureNames);
        Assert.IsNotNull(diffuseMaps);
        Assert.IsNotNull(normalMaps);
        Assert.IsNotNull(maskMaps);
        Assert.IsNotNull(detectBlackMaskShader);

        for (int i=0; i<textureNames.Count; ++i) {
            Assert.IsNotNull(textureNames[i]);
            Assert.IsNotNull(diffuseMaps[i]);
            Assert.IsNotNull(normalMaps[i]);
            Assert.IsNotNull(maskMaps[i]);
        }

        groundTextureCollections = new List<GroundTextureCollection>();
        indexOfDetectBlackMaskKernel = detectBlackMaskShader.FindKernel("CSMain");
    }

    public void AddGroundTexturesFromFile()
    {
        string[] paths = SFB.StandaloneFileBrowser.OpenFilePanel("Insert ground textures to the scene", "", "geojson", true);

        foreach (string path in paths) {
            try {
                GeoJSON.Net.Feature.FeatureCollection featureCollection = GeoJSONParser.FileToFeatureCollection(path);
            
                bool atLeastOneValidFeature = false;
                foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
                    string geometryType = feature.Geometry.GetType().FullName;

                    // Only Polygon and MultiPolygon are supported for now
                    if (feature.Properties.Count > 0 && string.Equals(geometryType, "GeoJSON.Net.Geometry.Polygon") || string.Equals(geometryType, "GeoJSON.Net.Geometry.MultiPolygon")) {
                        List<GeoJSON.Net.Geometry.Polygon> polygons = new List<GeoJSON.Net.Geometry.Polygon>();

                        if (string.Equals(geometryType, "GeoJSON.Net.Geometry.Polygon")) {
                            polygons.Add(feature.Geometry as GeoJSON.Net.Geometry.Polygon);
                        } else {
                            GeoJSON.Net.Geometry.MultiPolygon multiPolygon = feature.Geometry as GeoJSON.Net.Geometry.MultiPolygon;
                            polygons.AddRange(multiPolygon.Coordinates);
                        }

                        foreach (GeoJSON.Net.Geometry.Polygon polygon in polygons) {
                            if (polygon.Coordinates[0].Coordinates.Count > 1 && Utils.IsEPSG4326(polygon.Coordinates[0].Coordinates[0])) {
                                atLeastOneValidFeature = true;
                            }
                        }
                    }
                }

                if (atLeastOneValidFeature) {
                    changesUnsaved = true;
                    StartCoroutine(DisplayGroundTextureCollection(CreateGroundTextureFromFeatureCollection(featureCollection)));
                } else {
                    string message = "Ground texture not added.\n";
                    message += "The GeoJSON file should be made of Polygon or MultiPolygon with one property containing the ground type (";
                    foreach (string textureName in textureNames) {
                        message += textureName + ", ";
                    }
                    message = message.Remove(message.Length-2, 2);
                    message += ").\n";
                    message += "The EPSG:4326 coordinate system should be used (longitude from -180° to 180° / latitude from -90° to 90°).";

                    DialogControl.CreateDialog(message);
                }
            } catch (System.Exception e) {
                DialogControl.CreateDialog(e.Message);
            }
        }
    }

    public void SetBaseGroundToMaterials(string groundTextureName, List<Material> materials)
    {
        List<Tile> tiles = terrainManager.GetTiles();
        int textureIndex = textureNames.IndexOf(groundTextureName);
        Vector2 tiling;
        Texture2D diffuseMap;
        Texture2D normalMap;
        Texture2D maskMap;

        if (textureIndex > -1) {
            tiling = new Vector2(GROUND_TILING, GROUND_TILING);
            diffuseMap = diffuseMaps[textureIndex];
            normalMap = normalMaps[textureIndex];
            maskMap = maskMaps[textureIndex];
        } else {
            tiling = new Vector2(1, 1);
            diffuseMap = null;
            normalMap = null;
            maskMap = baseMaskMap;
        }

        foreach (Material material in materials) {
            material.SetVector(TILING_BASE_PROPERTY, tiling);
            material.SetTexture(DIFFUSE_BASE_PROPERTY, diffuseMap);
            material.SetTexture(NORMAL_MAP_BASE_PROPERTY, normalMap);
            material.SetTexture(MASK_MAP_BASE_PROPERTY, maskMap);
        }
    }

    public string GetPositionTexture(Material material, Vector2 position)
    {
        for (int i=0; i<MAX_NUMBER_OF_TEXTURES_PER_MATERIAL; ++i) {
            Texture2D mask = material.GetTexture(MASK_PROPERTY + i.ToString()) as Texture2D;

            // If the pixel of the mask is white, then this is the good ground texture (otherwise it's black)
            if (mask != null && mask.GetPixel((int) (position.x * mask.width), (int) (position.y * mask.height)) == Color.white) {
                return textureNames[(int) material.GetFloat(TEXTURE_PROPERTY + i.ToString())];
            }
        }
        return "";
    }

    public List<string> GetTextureNames()
    {
        return textureNames;
    }

    protected override void CreateObject(GeoJSON.Net.Feature.Feature feature, Location location)
    {
        string content = "";
        if (feature.Properties.ContainsKey("content")) {
            content = feature.Properties["content"] as string;
        }

        string id = "";
        if (feature.Properties.ContainsKey("id")) {
            id = feature.Properties["id"] as string;
        }

        if (content != "") {
            StartCoroutine(DisplayGroundTextureCollection(CreateGroundTextureFromFeatureCollection(GeoJSONParser.StringToFeatureCollection(content), id)));
        }
    }

    protected override void ClearActiveObjects()
    {
        ResetAllTileTextures();
        groundTextureCollections.Clear();
    }

    protected override void OnAfterLocationChanged()
    {}

    protected override List<GeoJSON.Net.Feature.Feature> GetFeaturesOfCurrentLocation()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (GroundTextureCollection groundTextureCollection in groundTextureCollections) {
            if (groundTextureCollection.Location != null) {
                GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(0, 0, 0));

                Dictionary<string, object> properties = new Dictionary<string, object> {
                    {"type", "groundTexture"},
                    {"location", groundTextureCollection.Location.Name},
                    {"content", GeoJSONParser.FeatureCollectionToString(groundTextureCollection.FeatureCollection)},
                    {"id", groundTextureCollection.Id}
                };

                features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
            }
        }

        return features;
    }

    private GroundTextureCollection CreateGroundTextureFromFeatureCollection(GeoJSON.Net.Feature.FeatureCollection featureCollection, string id = "")
    {
        GroundTextureCollection groundTextureCollection = CreateGroundTextureCollectionFromFeatureCollection(featureCollection, id);
        groundTextureCollections.Add(groundTextureCollection);
        return groundTextureCollection;
    }

    private void ResetAllTileTextures()
    {
        List<Tile> tiles = terrainManager.GetTiles();
        foreach (Tile tile in tiles) {
            for (int i=0; i<MAX_NUMBER_OF_TEXTURES_PER_MATERIAL; ++i) {
                // The mask is destroyed because otherwise would create a memory leak
                // The other textures may still be used by other tiles, so we don't destroy them
                Destroy(tile.MeshRenderer.material.GetTexture(MASK_PROPERTY + i.ToString()));
                tile.MeshRenderer.material.SetTexture(MASK_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetTexture(DIFFUSE_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetTexture(NORMAL_MAP_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetTexture(MASK_MAP_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetFloat(TEXTURE_PROPERTY + i.ToString(), -1);
            }
        }
    }

    private IEnumerator DisplayGroundTextureCollection(GroundTextureCollection groundTextureCollection)
    {
        yield return groundTextureMasksManager.CreateMasksIfDontExist(groundTextureCollection);
        SetMasks(groundTextureCollection);
    }

    private GroundTextureCollection CreateGroundTextureCollectionFromFeatureCollection(GeoJSON.Net.Feature.FeatureCollection featureCollection, string id)
    {
        GroundTextureCollection groundTextureCollection = new GroundTextureCollection(
            featureCollection,
            id,
            locationsManager.GetCurrentLocation()
        );

        foreach (GeoJSON.Net.Feature.Feature feature in featureCollection.Features) {
            string texture = feature.Properties.Values.First() as string;
            
            List<GeoJSON.Net.Geometry.Polygon> polygons = new List<GeoJSON.Net.Geometry.Polygon>();

            if (string.Equals(feature.Geometry.GetType().FullName, "GeoJSON.Net.Geometry.Polygon")) {
                polygons.Add(feature.Geometry as GeoJSON.Net.Geometry.Polygon);
            } else {
                GeoJSON.Net.Geometry.MultiPolygon multiPolygon = feature.Geometry as GeoJSON.Net.Geometry.MultiPolygon;
                polygons.AddRange(multiPolygon.Coordinates);
            }

            foreach (GeoJSON.Net.Geometry.Polygon polygon in polygons) {
                if (polygon.Coordinates.Count > 0 && polygon.Coordinates[0].Coordinates.Count > 3) {
                    GroundTexture groundTexture = new GroundTexture(texture, polygon.Coordinates);
                    groundTextureCollection.GroundTextures.Add(groundTexture);
                }
            }
        }

        return groundTextureCollection;
    }

    public void SetMasks(GroundTextureCollection groundTextureCollection)
    {
        List<Tile> tiles = terrainManager.GetTiles();

        foreach (Tile tile in tiles) {
            string path = System.IO.Path.Combine(groundTextureMasksManager.GetGroundTextureCollectionPath(tile.Id, groundTextureCollection.Id));
            string[] files = System.IO.Directory.GetFiles(path);

            // Determine which (if any) ground textures are already applied to this tile
            Dictionary<string, int> textureNamesToTextureShaderIndex = new Dictionary<string, int>();
            for (int i=0; i<MAX_NUMBER_OF_TEXTURES_PER_MATERIAL; ++i) {
                int texture = (int) tile.MeshRenderer.material.GetFloat(TEXTURE_PROPERTY + i);

                if (texture >= 0 && texture < textureNames.Count) {
                    textureNamesToTextureShaderIndex[textureNames[texture]] = i;
                }
            }

            foreach (string file in files) {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(file).Split('.')[0];

                if (textureNames.Contains(textureName)) {
                    Texture2D newMask = new Texture2D(1, 1);
                    newMask.wrapMode = TextureWrapMode.Clamp;   // used to avoid artifacts on the edges of the tiles
                    newMask.LoadImage(System.IO.File.ReadAllBytes(file));

                    if (!IsMaskBlack(newMask)) {
                        // Find where to set this ground texture on the material
                        int indexOfGroundTextureOnMaterial = -1;
                        if (textureNamesToTextureShaderIndex.ContainsKey(textureName)) {
                            indexOfGroundTextureOnMaterial = textureNamesToTextureShaderIndex[textureName];
                        } else {
                            indexOfGroundTextureOnMaterial = 0;
                            while (textureNamesToTextureShaderIndex.ContainsValue(indexOfGroundTextureOnMaterial)) {
                                indexOfGroundTextureOnMaterial++;
                            }

                            // If the ground texture is not already on the material, set it
                            if (indexOfGroundTextureOnMaterial < MAX_NUMBER_OF_TEXTURES_PER_MATERIAL) {
                                textureNamesToTextureShaderIndex[textureName] = indexOfGroundTextureOnMaterial;
                                int textureIndex = textureNames.FindIndex(x => x.Equals(textureName));
                                tile.MeshRenderer.material.SetTexture(DIFFUSE_PROPERTY + indexOfGroundTextureOnMaterial.ToString(), diffuseMaps[textureIndex]);
                                tile.MeshRenderer.material.SetTexture(NORMAL_MAP_PROPERTY + indexOfGroundTextureOnMaterial.ToString(), normalMaps[textureIndex]);
                                tile.MeshRenderer.material.SetTexture(MASK_MAP_PROPERTY + indexOfGroundTextureOnMaterial.ToString(), maskMaps[textureIndex]);
                                tile.MeshRenderer.material.SetFloat(TEXTURE_PROPERTY + indexOfGroundTextureOnMaterial.ToString(), textureNames.IndexOf(textureName));
                            } else {
                                indexOfGroundTextureOnMaterial = -1;
                            }
                        }

                        if (indexOfGroundTextureOnMaterial > -1) {
                            string textureShaderName = MASK_PROPERTY + indexOfGroundTextureOnMaterial;
                            Texture2D currentMask = tile.MeshRenderer.material.GetTexture(textureShaderName) as Texture2D;

                            // If the mask already exist, we combine it with the new one
                            if (currentMask != null) {
                                groundTextureMasksManager.CombineMasks(newMask, currentMask);
                            }
                            tile.MeshRenderer.material.SetTexture(textureShaderName, newMask);
                        }
                    }
                }
            }
        }
    }

    private bool IsMaskBlack(Texture2D mask)
    {
        int[] result = new int[1];
        result[0] = 0;

        ComputeBuffer computeBuffer = new ComputeBuffer(1, 4);
        computeBuffer.SetData(result);

        detectBlackMaskShader.SetTexture(indexOfDetectBlackMaskKernel, "Mask", mask);
        detectBlackMaskShader.SetBuffer(indexOfDetectBlackMaskKernel, "Result", computeBuffer);
        detectBlackMaskShader.Dispatch(
            indexOfDetectBlackMaskKernel,
            groundTextureMasksManager.GetMaskSize() / 32,
            groundTextureMasksManager.GetMaskSize() / 32,
            1
        );

        computeBuffer.GetData(result);
        bool maskBlack = result[0] == 0;
        computeBuffer.Release();

        return maskBlack;
    }
}