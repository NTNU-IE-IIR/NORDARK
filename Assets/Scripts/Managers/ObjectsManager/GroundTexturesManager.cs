using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers;

public class GroundTexturesManager : MonoBehaviour, IObjectsManager
{
    private static int[] textures = {1, 2};
    private const int RENDER_TEXTURE_SIZE = 512;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private Material meshMaskMaterial;
    [SerializeField] private GameObject maskCameraPrefab;
    private List<GroundTexture> groundTextures;
    
    void Awake()
    {
        Assert.IsNotNull(mapManager);
        Assert.IsNotNull(meshMaskMaterial);
        Assert.IsNotNull(maskCameraPrefab);

        groundTextures = new List<GroundTexture>();
    }

    public void Create(Feature feature)
    {
        GroundTexture groundTexture = new GroundTexture(System.Convert.ToInt32(feature.Properties["texture"]));
        for (int i=0; i<feature.Coordinates.Count; i++) {
            groundTexture.Coordinates.Add(new Vector2d(feature.Coordinates[i].x, feature.Coordinates[i].y));
        }
        groundTextures.Add(groundTexture);

        StartCoroutine(SetMasks());
    }

    public void Clear()
    {
        List<MeshRenderer> tiles = mapManager.GetTilesMeshRenderer();
        foreach (MeshRenderer tile in tiles) {
            foreach(int texture in textures) {
                tile.material.SetTexture("_Mask" + texture.ToString(), Texture2D.blackTexture);
            }
        }
        groundTextures.Clear();
    }

    public void OnLocationChanged()
    {
        StartCoroutine(SetMasks());
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();
        foreach (GroundTexture groundTexture in groundTextures) {
            Feature feature = new Feature();
            feature.Properties.Add("type", "groundTexture");
            feature.Properties.Add("texture", groundTexture.Texture);

            foreach (Vector2d coordinate in groundTexture.Coordinates) {
                feature.Coordinates.Add(new Vector3d(coordinate.y, coordinate.x, 0));
            }

            features.Add(feature);
        }
        return features;
    }

    private IEnumerator SetMasks()
    {
        foreach (GroundTexture groundTexture in groundTextures) {
            CreateMaskMesh(groundTexture);
            yield return CreateMask(groundTexture);
            DeleteMaskMesh();
        }
    }

    private void CreateMaskMesh(GroundTexture groundTexture)
    {
        List<Vector3> points = groundTexture.Coordinates.Select(
            coordinate => mapManager.GetUnityPositionFromCoordinates(new Vector3d(coordinate, 0))
        ).ToList();

        var flatData = EarcutLibrary.Flatten(new List<List<Vector3>>() { points });
		var triangles = EarcutLibrary.Earcut(flatData.Vertices, flatData.Holes, flatData.Dim);

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = meshMaskMaterial;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = points.Select(point => Vector3.up).ToArray();
        meshFilter.mesh = mesh;
    }

    private IEnumerator CreateMask(GroundTexture groundTexture)
    {
        List<Mapbox.Unity.MeshGeneration.Data.UnityTile> tiles = mapManager.GetTiles();

        Camera maskCamera = Instantiate(maskCameraPrefab).GetComponent<Camera>();
        RenderTexture renderTexture = new RenderTexture(RENDER_TEXTURE_SIZE, RENDER_TEXTURE_SIZE, 0);
        maskCamera.targetTexture = renderTexture;

        foreach (Mapbox.Unity.MeshGeneration.Data.UnityTile tile in tiles) {
            maskCamera.transform.position = new Vector3(
                tile.transform.position.x,
                maskCamera.transform.position.y,
                tile.transform.position.z
            );
            maskCamera.orthographicSize = tile.GetComponent<MeshFilter>().mesh.bounds.size.x / 2;

            yield return null;

            RenderTexture oldRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
                        
            RenderTexture.active = oldRenderTexture;

            tile.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_Mask" + groundTexture.Texture.ToString(), texture);
        }

        renderTexture.Release();
        Destroy(maskCamera.gameObject);
    }

    private void DeleteMaskMesh()
    {
        Destroy(gameObject.GetComponent<MeshRenderer>());
        Destroy(gameObject.GetComponent<MeshFilter>());
    }
}
