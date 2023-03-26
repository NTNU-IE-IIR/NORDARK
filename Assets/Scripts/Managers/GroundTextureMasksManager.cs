using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers;

public class GroundTextureMasksManager : MonoBehaviour
{
    private const string MASK_FOLDER = "ground-textures";
    private const int MASK_TEXTURE_SIZE = 512;
    [SerializeField] private GroundTexturesManager groundTexturesManager;
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private GroundTexturesWindow groundTexturesWindow;
    [SerializeField] private GameObject maskMeshPrefab;
    [SerializeField] private GameObject maskCameraPrefab;
    [SerializeField] private ComputeShader combineMasksShader;
    private int indexOfCombineMaskKernel;
    private int numberOfSteps;
    private int currentStep;

    void Awake()
    {
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(terrainManager);
        Assert.IsNotNull(groundTexturesWindow);
        Assert.IsNotNull(maskMeshPrefab);
        Assert.IsNotNull(maskCameraPrefab);
        Assert.IsNotNull(combineMasksShader);

        indexOfCombineMaskKernel = combineMasksShader.FindKernel("CSMain");
        numberOfSteps = 0;
        currentStep = 0;
    }

    public void AddMasksFromResources()
    {
        Utils.AddFolderFromResources(MASK_FOLDER);
    }

    public IEnumerator CreateMasksIfDontExist(GroundTextureCollection groundTextureCollection)
    {
        if (!AreMasksCreated(groundTextureCollection)) {
            List<Tile> tiles = terrainManager.GetTiles();
            List<GroundTexture> groundTexturesOnMap = new List<GroundTexture>();
            foreach (GroundTexture groundTexture in groundTextureCollection.GroundTextures) {
                if (IsGroundTextureOnMap(groundTexture)) {
                    groundTexturesOnMap.Add(groundTexture);
                }
            }

            if (groundTexturesOnMap.Count > 0) {
                groundTexturesWindow.Show(true);
                groundTexturesWindow.SetDescription("Adding ground textures...");
                numberOfSteps = tiles.Count * groundTexturesManager.GetTextureNames().Count;
                currentStep = 0;

                RenderTexture oldActiveRenderTexture = RenderTexture.active;

                foreach (string texture in groundTexturesManager.GetTextureNames()) {
                    yield return CreateTextureMasks(texture, groundTexturesOnMap, tiles, oldActiveRenderTexture, groundTextureCollection.Id);
                }

                groundTexturesWindow.Show(false);
            }
        }
    }
    
    public void CombineMasks(Texture2D newMask, Texture2D currentMask)
    {
        RenderTexture oldRenderTexture = RenderTexture.active;

        RenderTexture resultCombine = RenderTexture.GetTemporary(currentMask.width, currentMask.height, 0);
        resultCombine.enableRandomWrite = true;

        combineMasksShader.SetTexture(indexOfCombineMaskKernel, "Mask1", newMask);
        combineMasksShader.SetTexture(indexOfCombineMaskKernel, "Mask2", currentMask);
        combineMasksShader.SetTexture(indexOfCombineMaskKernel, "Result", resultCombine);
        combineMasksShader.Dispatch(indexOfCombineMaskKernel, resultCombine.width / 32, resultCombine.height / 32, 1);

        RenderTexture.active = resultCombine;
        newMask.ReadPixels(new Rect(0, 0, resultCombine.width, resultCombine.height), 0, 0);
        newMask.Apply();

        RenderTexture.active = oldRenderTexture;
        RenderTexture.ReleaseTemporary(resultCombine);
        Destroy(currentMask);
    }

    public string GetGroundTextureCollectionPath(string tileId, string groundTextureCollectionId)
    {
        string folderPath = System.IO.Path.Combine(Application.persistentDataPath, MASK_FOLDER, tileId, groundTextureCollectionId);
        System.IO.Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    public int GetMaskSize()
    {
        return MASK_TEXTURE_SIZE;
    }

    private bool AreMasksCreated(GroundTextureCollection groundTextureCollection)
    {
        List<Tile> tiles = terrainManager.GetTiles();
        foreach (Tile tile in tiles) {
            string path = System.IO.Path.Combine(Application.persistentDataPath, MASK_FOLDER, tile.Id, groundTextureCollection.Id);
            if (!System.IO.Directory.Exists(path)) {
                return false;
            }
        }
        return true;
    }

    private bool IsGroundTextureOnMap(GroundTexture groundTexture)
    {
        Vector3 mapBounds = terrainManager.GetMapSize();

        List<Vector3> points = groundTexture.Coordinates.Select(
            coordinate => terrainManager.GetUnityPositionFromCoordinates(coordinate)
        ).ToList();

        foreach (Vector3 point in points) {
            if (-mapBounds.x/2 <= point.x && point.x <= mapBounds.x/2 && -mapBounds.z/2 <= point.z && point.z <= mapBounds.z/2) {
                return true;
            }
        }

        return false;
    }

    private IEnumerator CreateTextureMasks(string texture, List<GroundTexture> groundTexturesOnMap, List<Tile> tiles, RenderTexture oldActiveRenderTexture, string groundTextureCollectionId)
    {
        List<GameObject> maskMeshes = CreateMaskMeshsOfTexture(groundTexturesOnMap, texture);

        Camera maskCamera = Instantiate(maskCameraPrefab, transform).GetComponent<Camera>();
        RenderTexture maskCameraTexture = new RenderTexture(MASK_TEXTURE_SIZE, MASK_TEXTURE_SIZE, 0);
        maskCamera.targetTexture = maskCameraTexture;
        if (tiles.Count > 0) {
            maskCamera.orthographicSize = tiles[0].MeshFilter.mesh.bounds.size.x / 2;
        }

        foreach (Tile tile in tiles) {
            maskCamera.transform.position = new Vector3(
                tile.Transform.position.x,
                maskCamera.transform.position.y,
                tile.Transform.position.z
            );

            yield return null;

            Texture2D newMask = new Texture2D(maskCameraTexture.width, maskCameraTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = maskCameraTexture;
            newMask.ReadPixels(new Rect(0, 0, maskCameraTexture.width, maskCameraTexture.height), 0, 0);
            newMask.Apply();
            RenderTexture.active = oldActiveRenderTexture;

            string maskPath = System.IO.Path.Combine(GetGroundTextureCollectionPath(tile.Id, groundTextureCollectionId), texture + ".png");
            if (System.IO.File.Exists(maskPath)) {
                Texture2D currentMask = new Texture2D(maskCameraTexture.width, maskCameraTexture.height);
                currentMask.LoadImage(System.IO.File.ReadAllBytes(maskPath));
                
                CombineMasks(newMask, currentMask);
            }

            System.IO.File.WriteAllBytes(maskPath, newMask.EncodeToPNG());
            Destroy(newMask);

            StepsDone();
        }

        maskCameraTexture.Release();
        Destroy(maskCamera.gameObject);

        foreach (GameObject maskMesh in maskMeshes) {
            Destroy(maskMesh);
        }
    }

    private List<GameObject> CreateMaskMeshsOfTexture(List<GroundTexture> groundTextures, string texture)
    {
        List<GameObject> maskMeshes = new List<GameObject>();

        foreach (GroundTexture groundTexture in groundTextures) {
            if (groundTexture.Texture == texture) {
                maskMeshes.Add(CreateAndReturnMaskMesh(groundTexture.Coordinates, false));

                foreach (List<Coordinate> holeCoordinates in groundTexture.HolesCoordinates) {
                    maskMeshes.Add(CreateAndReturnMaskMesh(holeCoordinates, true));
                }
            }
        }
        
        return maskMeshes;
    }

    private void StepsDone(int numberOfStepsDone = 1)
    {
        currentStep += numberOfStepsDone;
        groundTexturesWindow.SetProgress((float) currentStep / numberOfSteps);
    }

    private GameObject CreateAndReturnMaskMesh(List<Coordinate> coordinates, bool isHole)
    {
        List<Vector3> points = coordinates.Select(
            coordinate => terrainManager.GetUnityPositionFromCoordinates(coordinate)
        ).ToList();

        // Create triangles representing the polygon
        var flatData = EarcutLibrary.Flatten(new List<List<Vector3>>() { points });
        List<int> triangles = EarcutLibrary.Earcut(flatData.Vertices, flatData.Holes, flatData.Dim);

        GameObject maskMesh = Instantiate(maskMeshPrefab, transform);

        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = points.Select(point => Vector3.up).ToArray();
        maskMesh.GetComponent<MeshFilter>().mesh = mesh;
        maskMesh.GetComponent<MeshRenderer>().material.color = isHole ? Color.black : Color.white;
        
        // Place holes above exterior ring
        if (isHole) {
            maskMesh.transform.position += new Vector3(0, 1, 0);
        }

        return maskMesh;
    }
}
