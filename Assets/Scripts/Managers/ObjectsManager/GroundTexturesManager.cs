using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers;

public class GroundTexturesManager : MonoBehaviour, IObjectsManager
{
    private const string TEXTURE_PROPERTY = "_Texture";
    private const string DIFFUSE_PROPERTY = "_Diffuse";
    private const string NORMAL_MAP_PROPERTY = "_NormalMap";
    private const string HEIGHT_MAP_PROPERTY = "_HeightMap";
    private const string MASK_MAP_PROPERTY = "_MaskMap";
    private const string MASK_PROPERTY = "_Mask";
    private const string MASK_FOLDER = "ground-textures";
    private const int MASK_TEXTURE_SIZE = 512;
    private const int NUMBER_OF_TEXTURES = 8;
    private const int MAX_NUMBER_OF_COROUTINES = 10;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private SceneManager sceneManager;
    [SerializeField] private DialogControl dialogControl;
    [SerializeField] private GroundTexturesWindow groundTexturesWindow;
    [SerializeField] private GameObject maskMeshPrefab;
    [SerializeField] private GameObject maskCameraPrefab;
    [SerializeField] private ComputeShader combineMasksShader;
    [SerializeField] private ComputeShader detectBlackMaskShader;
    [SerializeField] private List<string> textureNames;
    [SerializeField] private List<Texture2D> diffuseMaps;
    [SerializeField] private List<Texture2D> normalMaps;
    [SerializeField] private List<Texture2D> heightMaps;
    [SerializeField] private List<Texture2D> maskMaps;
    private List<GroundTextureCollection> groundTextureCollections;
    private List<float> offsets;
    private int indexOfCombineMaskKernel;
    private int indexOfDetectBlackMaskKernel;
    private int numberOfTextures;
    private int numberOfSteps;
    private int currentStep;
    
    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(dialogControl);
        Assert.IsNotNull(groundTexturesWindow);
        Assert.IsNotNull(maskMeshPrefab);
        Assert.IsNotNull(maskCameraPrefab);
        Assert.IsNotNull(combineMasksShader);
        Assert.IsNotNull(detectBlackMaskShader);
        Assert.IsNotNull(textureNames);
        Assert.IsNotNull(diffuseMaps);
        Assert.IsNotNull(normalMaps);
        Assert.IsNotNull(heightMaps);
        Assert.IsNotNull(maskMaps);

        for (int i=0; i<textureNames.Count; ++i) {
            Assert.IsNotNull(textureNames[i]);
            Assert.IsNotNull(diffuseMaps[i]);
            Assert.IsNotNull(normalMaps[i]);
            Assert.IsNotNull(heightMaps[i]);
            Assert.IsNotNull(maskMaps[i]);
        }

        groundTextureCollections = new List<GroundTextureCollection>();
        offsets = new List<float>();
        indexOfCombineMaskKernel = combineMasksShader.FindKernel("CSMain");
        indexOfDetectBlackMaskKernel = detectBlackMaskShader.FindKernel("CSMain");
        numberOfTextures = diffuseMaps.Count;
        numberOfSteps = 0;
        currentStep = 0;
    }

    public void Create(GeoJSON.Net.Feature.Feature feature)
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
            CreateGroundTextureFromFeatureCollection(GeoJSONParser.StringToFeatureCollection(content), id);
        }
    }

    public void Clear()
    {
        ResetAllTileTextures();
        groundTextureCollections.Clear();
    }

    public void OnLocationChanged()
    {
        ResetAllTileTextures();

        foreach (GroundTextureCollection groundTextureCollection in groundTextureCollections) {
            StartCoroutine(DisplayGroundTextureCollection(groundTextureCollection));
        }
    }

    public List<GeoJSON.Net.Feature.Feature> GetFeatures()
    {
        List<GeoJSON.Net.Feature.Feature> features = new List<GeoJSON.Net.Feature.Feature>();

        foreach (GroundTextureCollection groundTextureCollection in groundTextureCollections) {
            GeoJSON.Net.Geometry.IGeometryObject geometry = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.Position(0, 0, 0));

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("type", "groundTexture");
            properties.Add("content", GeoJSONParser.FeatureCollectionToString(groundTextureCollection.FeatureCollection));
            properties.Add("id", groundTextureCollection.Id);

            features.Add(new GeoJSON.Net.Feature.Feature(geometry, properties));
        }

        return features;
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

                    dialogControl.CreateInfoDialog(message);
                }
            } catch (System.Exception e) {
                dialogControl.CreateInfoDialog(e.Message);
            }
        }
    }

    private GroundTextureCollection CreateGroundTextureFromFeatureCollection(GeoJSON.Net.Feature.FeatureCollection featureCollection, string id = "")
    {
        GroundTextureCollection groundTextureCollection = CreateGroundTextureCollectionFromFeatureCollection(featureCollection, id);
        groundTextureCollections.Add(groundTextureCollection);
        return groundTextureCollection;
    }

    private void ResetAllTileTextures()
    {
        List<Tile> tiles = mapManager.GetTiles();
        foreach (Tile tile in tiles) {
            for (int i=1; i<=NUMBER_OF_TEXTURES; ++i) {
                Destroy(tile.MeshRenderer.material.GetTexture(MASK_PROPERTY + i.ToString()));
                tile.MeshRenderer.material.SetTexture(MASK_PROPERTY + i.ToString(), null);

                tile.MeshRenderer.material.SetTexture(DIFFUSE_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetTexture(NORMAL_MAP_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetTexture(HEIGHT_MAP_PROPERTY + i.ToString(), null);
                tile.MeshRenderer.material.SetTexture(MASK_MAP_PROPERTY + i.ToString(), null);
            }
        }
    }

    private IEnumerator DisplayGroundTextureCollection(GroundTextureCollection groundTextureCollection)
    {
        if (!AreMasksCreated(groundTextureCollection)) {
            yield return CreateMasks(groundTextureCollection);
        }
        
        SetMasks(groundTextureCollection);
    }

    private GroundTextureCollection CreateGroundTextureCollectionFromFeatureCollection(GeoJSON.Net.Feature.FeatureCollection featureCollection, string id)
    {
        GroundTextureCollection groundTextureCollection = new GroundTextureCollection(featureCollection, id);

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
                if (polygon.Coordinates[0].Coordinates.Count > 1) {
                    GroundTexture groundTexture = new GroundTexture(texture, polygon.Coordinates[0].Coordinates);
                    groundTextureCollection.GroundTextures.Add(groundTexture);
                }
            }
        }

        return groundTextureCollection;
    }

    private bool AreMasksCreated(GroundTextureCollection groundTextureCollection)
    {
        List<Tile> tiles = mapManager.GetTiles();
        foreach (Tile tile in tiles) {
            string path = System.IO.Path.Combine(Application.persistentDataPath, MASK_FOLDER, tile.Id, groundTextureCollection.Id);
            if (!System.IO.Directory.Exists(path)) {
                return false;
            }
        }
        return true;
    }

    private IEnumerator CreateMasks(GroundTextureCollection groundTextureCollection)
    {
        List<Tile> tiles = mapManager.GetTiles();

        groundTexturesWindow.Show(true);
        groundTexturesWindow.SetDescription("Adding ground textures...");
        numberOfSteps = tiles.Count * groundTextureCollection.GroundTextures.Count;
        currentStep = 0;

        int index = 0;
        while (index < groundTextureCollection.GroundTextures.Count) {
            yield return CreateNextMasks(groundTextureCollection, tiles, index);
            index += MAX_NUMBER_OF_COROUTINES;
        }

        groundTexturesWindow.Show(false);
    }

    private void SetMasks(GroundTextureCollection groundTextureCollection)
    {
        RenderTexture oldRenderTexture = RenderTexture.active;
        Texture2D currentMask;

        RenderTexture resultCombine = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0);
        resultCombine.enableRandomWrite = true;
        resultCombine.Create();

        List<Tile> tiles = mapManager.GetTiles();

        foreach (Tile tile in tiles) {
            string path = System.IO.Path.Combine(GetGroundTextureCollectionPath(tile.Id, groundTextureCollection.Id));
            string[] files = System.IO.Directory.GetFiles(path);

            Dictionary<string, int> textureNamesToTextureShaderIndex = new Dictionary<string, int>();
            for (int i=1; i<=NUMBER_OF_TEXTURES; ++i) {
                int texture = (int) tile.MeshRenderer.material.GetFloat(TEXTURE_PROPERTY + i);

                if (texture >= 0 && texture < numberOfTextures) {
                    textureNamesToTextureShaderIndex[textureNames[texture]] = i;
                }
            }

            foreach (string file in files) {
                string textureName = System.IO.Path.GetFileNameWithoutExtension(file).Split('.')[0];

                if (textureNames.Contains(textureName)) {
                    Texture2D newMask = new Texture2D(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE);
                    newMask.LoadImage(System.IO.File.ReadAllBytes(file));

                    if (!IsMaskBlack(newMask)) {
                        string textureShaderName = MASK_PROPERTY;
                        if (textureNamesToTextureShaderIndex.ContainsKey(textureName)) {
                            textureShaderName += textureNamesToTextureShaderIndex[textureName];
                        } else {
                            int i = 1;
                            while (textureNamesToTextureShaderIndex.ContainsValue(i)) {
                                i++;
                            }

                            if (i <= NUMBER_OF_TEXTURES) {
                                textureShaderName += i.ToString();
                                textureNamesToTextureShaderIndex[textureName] = i;
                                
                                int textureIndex = textureNames.FindIndex(x => x.Equals(textureName));
                                tile.MeshRenderer.material.SetTexture(DIFFUSE_PROPERTY + i.ToString(), diffuseMaps[textureIndex]);
                                tile.MeshRenderer.material.SetTexture(NORMAL_MAP_PROPERTY + i.ToString(), normalMaps[textureIndex]);
                                tile.MeshRenderer.material.SetTexture(HEIGHT_MAP_PROPERTY + i.ToString(), heightMaps[textureIndex]);
                                tile.MeshRenderer.material.SetTexture(MASK_MAP_PROPERTY + i.ToString(), maskMaps[textureIndex]);
                            } else {
                                textureShaderName = "";
                            }
                        }

                        if (textureShaderName != "") {
                            currentMask = tile.MeshRenderer.material.GetTexture(textureShaderName) as Texture2D;
                            if (currentMask != null) {
                                CombineMasks(resultCombine, newMask, currentMask);
                            }
                            tile.MeshRenderer.material.SetTexture(textureShaderName, newMask);
                        }
                    }
                }
            }
        }

        RenderTexture.active = oldRenderTexture;
        resultCombine.Release();
    }

    private IEnumerator CreateNextMasks(GroundTextureCollection groundTextureCollection, List<Tile> tiles, int index)
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        int i = index;
        while (i < groundTextureCollection.GroundTextures.Count && i < index + MAX_NUMBER_OF_COROUTINES) {
            coroutines.Add(StartCoroutine(CreateMask(groundTextureCollection, groundTextureCollection.GroundTextures[i], tiles)));
            i++;
        }

        foreach (Coroutine coroutine in coroutines) {
            yield return coroutine;
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
        detectBlackMaskShader.Dispatch(indexOfDetectBlackMaskKernel, MASK_TEXTURE_SIZE / 32, MASK_TEXTURE_SIZE / 32, 1);

        computeBuffer.GetData(result);
        bool maskBlack = result[0] == 0;
        computeBuffer.Release();

        return maskBlack;
    }

    private void CombineMasks(RenderTexture resultCombine, Texture2D newMask, Texture2D currentMask)
    {
        combineMasksShader.SetTexture(indexOfCombineMaskKernel, "Mask1", newMask);
        combineMasksShader.SetTexture(indexOfCombineMaskKernel, "Mask2", currentMask);
        combineMasksShader.SetTexture(indexOfCombineMaskKernel, "Result", resultCombine);
        combineMasksShader.Dispatch(indexOfCombineMaskKernel, resultCombine.width / 32, resultCombine.height / 32, 1);

        RenderTexture.active = resultCombine;
        newMask.ReadPixels(new Rect(0, 0, resultCombine.width, resultCombine.height), 0, 0);
        newMask.Apply();

        Destroy(currentMask);
    }

    private IEnumerator CreateMask(GroundTextureCollection groundTextureCollection, GroundTexture groundTexture, List<Tile> tiles)
    {
        if (IsGroundTextureOnMap(groundTexture)) {
            RenderTexture oldRenderTexture = RenderTexture.active;

            RenderTexture resultCombine = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0);
            resultCombine.enableRandomWrite = true;
            resultCombine.Create();

            Camera maskCamera = Instantiate(maskCameraPrefab, transform).GetComponent<Camera>();
            RenderTexture maskCameraTexture = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0);
            maskCamera.targetTexture = maskCameraTexture;

            GameObject maskMesh = CreateMaskMesh(groundTexture);

            foreach (Tile tile in tiles) {
                maskCamera.transform.position = new Vector3(
                    tile.Transform.position.x + maskMesh.transform.position.x,
                    maskCamera.transform.position.y,
                    tile.Transform.position.z + maskMesh.transform.position.z
                );
                maskCamera.orthographicSize = tile.MeshFilter.mesh.bounds.size.x / 2;

                yield return null;

                Texture2D newMask = new Texture2D(maskCameraTexture.width, maskCameraTexture.height, TextureFormat.RGB24, false);
                RenderTexture.active = maskCameraTexture;
                newMask.ReadPixels(new Rect(0, 0, maskCameraTexture.width, maskCameraTexture.height), 0, 0);
                newMask.Apply();

                string maskPath = System.IO.Path.Combine(GetGroundTextureCollectionPath(tile.Id, groundTextureCollection.Id), groundTexture.Texture + ".png");
                if (System.IO.File.Exists(maskPath)) {
                    Texture2D currentMask = new Texture2D(maskCameraTexture.width, maskCameraTexture.height);
                    currentMask.LoadImage(System.IO.File.ReadAllBytes(maskPath));
                    
                    CombineMasks(resultCombine, newMask, currentMask);
                }

                System.IO.File.WriteAllBytes(maskPath, newMask.EncodeToPNG());
                Destroy(newMask);

                StepsDone();
            }
            DeleteMaskMesh(maskMesh);
            
            RenderTexture.active = oldRenderTexture;
            resultCombine.Release();
            maskCameraTexture.Release();
            Destroy(maskCamera.gameObject);
        } else {
            StepsDone(tiles.Count);
        }
    }

    private bool IsGroundTextureOnMap(GroundTexture groundTexture)
    {
        Vector3 mapBounds = mapManager.GetMapSize();

        List<Vector3> points = groundTexture.Coordinates.Select(
            coordinate => mapManager.GetUnityPositionFromCoordinates(new Vector3d(coordinate))
        ).ToList();

        foreach (Vector3 point in points) {
            if (-mapBounds.x/2 <= point.x && point.x <= mapBounds.x/2 && -mapBounds.z/2 <= point.z && point.z <= mapBounds.z/2) {
                return true;
            }
        }

        return false;
    }

    private GameObject CreateMaskMesh(GroundTexture groundTexture)
    {
        List<Vector3> points = groundTexture.Coordinates.Select(
            coordinate => mapManager.GetUnityPositionFromCoordinates(new Vector3d(coordinate))
        ).ToList();

        var flatData = EarcutLibrary.Flatten(new List<List<Vector3>>() { points });
		List<int> triangles = EarcutLibrary.Earcut(flatData.Vertices, flatData.Holes, flatData.Dim);

        GameObject maskMesh = Instantiate(maskMeshPrefab, transform);
        float offset = 0;
        if (offsets.Count > 0) {
            Vector3 mapBounds = mapManager.GetMapSize();
            offset = offsets.Max() + Mathf.Max(mapBounds.x, mapBounds.z);
        }
        
        maskMesh.transform.position += new Vector3(offset, 0, offset);
        offsets.Add(offset);

        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = points.Select(point => Vector3.up).ToArray();
        maskMesh.GetComponent<MeshFilter>().mesh = mesh;
        
        return maskMesh;
    }

    private string GetGroundTextureCollectionPath(string tileId, string groundTextureCollectionId)
    {
        string folderPath = System.IO.Path.Combine(Application.persistentDataPath, MASK_FOLDER, tileId, groundTextureCollectionId);
        System.IO.Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    private void StepsDone(int numberOfStepsDone = 1)
    {
        currentStep += numberOfStepsDone;
        groundTexturesWindow.SetProgress((float) currentStep / numberOfSteps);
    }

    private void DeleteMaskMesh(GameObject maskMesh)
    {
        offsets.Remove(maskMesh.transform.position.x);
        Destroy(maskMesh);
    }
}