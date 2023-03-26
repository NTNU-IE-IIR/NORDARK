using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainTypeManager : MonoBehaviour
{
    public abstract void ChangeLocation(Location location);
    public abstract Vector3 GetUnityPositionFromCoordinates(Coordinate coordinates, bool stickToGround = false);
    public abstract Coordinate GetCoordinatesFromUnityPosition(Vector3 position);
    public abstract bool IsCoordinateOnMap(Coordinate coordinates);
    public abstract List<Tile> GetTiles();
    public abstract Vector3 GetMapSize();
    public abstract string GetGroundFromPosition(Vector3 position);
    public abstract void SetStyle(int styleIndex);
    public abstract void DisplayBuildings(bool display);
    public abstract Location.TerrainType TerrainType { get; set; }
    [SerializeField] protected SceneManager sceneManager;
    [SerializeField] protected LocationsManager locationsManager;
    [SerializeField] protected SkyManager skyManager;
    [SerializeField] protected LightComputationManager lightComputationManager;
    [SerializeField] protected SceneCamerasManager sceneCamerasManager;
    [SerializeField] protected GroundTexturesManager groundTexturesManager;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    protected void OnLocationChanged()
    {
        sceneManager.SendOnLocationChangedToAllObjects();
        locationsManager.OnLocationChanged();
        skyManager.OnLocationChanged();
        lightComputationManager.OnLocationChanged();
        sceneCamerasManager.OnLocationChanged();
    }
}
