using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TreeManager : MonoBehaviour
{
    [SerializeField]
    private GameObject treePrefab;
    [SerializeField]
    private MapManager mapManager;

    void Awake()
    {
        Assert.IsNotNull(treePrefab);
        Assert.IsNotNull(mapManager);
    }

    public void CreateTree(Vector2d latLong, double altitude)
    {
        Vector3 position = mapManager.GetUnityPositionFromCoordinatesAndAltitude(latLong, altitude, true);
        GameObject tree = Instantiate(treePrefab, position, Quaternion.identity, transform);
        tree.transform.localScale *= mapManager.GetWorldRelativeScale();
    }

    public void ClearTrees()
    {
        foreach (Transform child in transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    public void UpdateTreesPosition()
    {
        foreach (Transform child in transform) {
            Vector2d coordinates = mapManager.GetCoordinatesFromUnityPosition(child.gameObject.transform.position);
            Vector3 position = mapManager.GetUnityPositionFromCoordinatesAndAltitude(coordinates, 0, true);
            child.gameObject.transform.position = position;
        }
    }

    public List<Feature> GetFeatures()
    {
        List<Feature> features = new List<Feature>();

        foreach (Transform child in transform) {
            Feature feature = new Feature();
            feature.Properties.Add("type", "tree");

            feature.Coordinates = new Vector3d(
                mapManager.GetCoordinatesFromUnityPosition(child.position),
                mapManager.GetAltitudeFromUnityPosition(child.position)
            );
            features.Add(feature);
        }

        return features;
    }
}
