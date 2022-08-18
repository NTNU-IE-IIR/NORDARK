using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;

public class MapManager : MonoBehaviour
{
    public const int UNITY_LAYER_MAP = 8;

    [SerializeField]
    private LightsManager lightsManager;

    private AbstractMap map;

    void Awake()
    {
        Assert.IsNotNull(lightsManager);

        map = GetComponent<AbstractMap>();
        map.Terrain.AddToUnityLayer(UNITY_LAYER_MAP);
    }

    public Vector3 GetUnityPositionFromCoordinates(Vector2 latLong)
    {
        return latLong.AsUnityPosition(map.CenterMercator, map.WorldRelativeScale);
    }

    public float GetElevationInUnityUnitsFromCoordinates(Mapbox.Utils.Vector2d latLong)
    {
        return map.QueryElevationInUnityUnitsAt(latLong);
    }

    public Mapbox.Utils.Vector2d GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return position.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale);
    }

    public void SetMapLocation(Mapbox.Utils.Vector2d coordinates)
    {
        map.SetCenterLatitudeLongitude(coordinates);
        map.UpdateMap();
        lightsManager.UpdateLightsPositions();
    }

    public Mapbox.Utils.Vector2d GetMapLocation()
    {
        return map.CenterLatitudeLongitude;
    }

    public void SetStyle(int style)
    {
        map.ImageLayer.SetLayerSource((ImagerySourceType) style);
    }

    public void SetElevation(bool enableElevation)
    {
        if (enableElevation) {
            map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
        } else {
            map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);
        }
        lightsManager.UpdateLightsPositions();
    }

    public float GetWorldRelativeScale()
    {
        return map.WorldRelativeScale;
    }

    public void DisplayBuildings(bool display)
    {
        map.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(display);
    }

    public void DisplayRoads(bool display)
    {
        map.VectorData.GetFeatureSubLayerAtIndex(1).SetActive(display);
    }
}
