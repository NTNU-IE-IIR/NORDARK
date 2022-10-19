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
  private List<TreeNode> trees;

  void Awake()
  {
    Assert.IsNotNull(treePrefab);
    Assert.IsNotNull(mapManager);

    trees = new List<TreeNode>();
  }

  public void CreateTree(TreeNode tree)
  {
    trees.Add(tree);
    Vector3 position = mapManager.GetUnityPositionFromCoordinatesAndAltitude(tree.LatLong, tree.Altitude, true);
    GameObject treeObject = Instantiate(treePrefab, position, Quaternion.identity, transform);
    treeObject.transform.localScale *= mapManager.GetWorldRelativeScale();
    tree.Tree = treeObject;
  }

  public void ClearTrees()
  {
    foreach (TreeNode tree in trees)
    {
      GameObject.Destroy(tree.Tree);
    }
    trees.Clear();
  }

  public void Show(bool show)
  {
    gameObject.SetActive(show);
  }

  public void UpdateTreesPosition()
  {
    foreach (TreeNode tree in trees)
    {
      tree.Tree.transform.position = mapManager.GetUnityPositionFromCoordinatesAndAltitude(tree.LatLong, tree.Altitude, true);
    }
  }

  public List<Feature> GetFeatures()
  {
    List<Feature> features = new List<Feature>();

    foreach (TreeNode tree in trees) {
        Feature feature = new Feature();
        feature.Properties.Add("type", "tree");

        feature.Coordinates = new Vector3d(tree.LatLong, tree.Altitude);
        features.Add(feature);
    }

    return features;
  }
}
