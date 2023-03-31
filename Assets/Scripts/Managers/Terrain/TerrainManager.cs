using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TerrainManager : MonoBehaviour
{
    public const int TERRAIN_LAYER = 6;
    [SerializeField] private List<TerrainTypeManager> terrainTypeManagers;
    private Location.TerrainType terrainType;

    void Awake()
    {
        Assert.IsNotNull(terrainTypeManagers);
    }

    public void ChangeLocation(Location location)
    {
        if (location != null) {
            terrainType = location.Type;

            foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
                if (terrainTypeManager.TerrainType == terrainType) {
                    terrainTypeManager.SetActive(true);
                    terrainTypeManager.ChangeLocation(location);
                } else {
                    terrainTypeManager.SetActive(false);
                }
            }
        } else {
            foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
                terrainTypeManager.SetActive(false);
            }
        }
    }

    public Vector3 GetUnityPositionFromCoordinates(Coordinate coordinates, bool stickToGround = false)
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            if (terrainTypeManager.TerrainType == terrainType) {
                return terrainTypeManager.GetUnityPositionFromCoordinates(coordinates, stickToGround);
            }
        }
        return new Vector3();
    }

    public Coordinate GetCoordinatesFromUnityPosition(Vector3 position)
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            if (terrainTypeManager.TerrainType == terrainType) {
                return terrainTypeManager.GetCoordinatesFromUnityPosition(position);
            }
        }
        return new Coordinate();
    }

    public bool IsCoordinateOnMap(Coordinate coordinates)
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            if (terrainTypeManager.TerrainType == terrainType) {
                return terrainTypeManager.IsCoordinateOnMap(coordinates);
            }
        }
        return false;
    }

    public List<Tile> GetTiles()
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            if (terrainTypeManager.TerrainType == terrainType) {
                return terrainTypeManager.GetTiles();
            }
        }
        return new List<Tile>();
    }

    public Vector3 GetMapSize()
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            if (terrainTypeManager.TerrainType == terrainType) {
                return terrainTypeManager.GetMapSize();
            }
        }
        return new Vector3();
    }

    public string GetGroundFromPosition(Vector3 position)
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            if (terrainTypeManager.TerrainType == terrainType) {
                return terrainTypeManager.GetGroundFromPosition(position);
            }
        }
        return "";
    }

    public void SetStyle(int styleIndex)
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            terrainTypeManager.SetStyle(styleIndex);
        }
    }
    
    public void DisplayBuildings(bool display)
    {
        foreach (TerrainTypeManager terrainTypeManager in terrainTypeManagers) {
            terrainTypeManager.DisplayBuildings(display);
        }
    }
}
