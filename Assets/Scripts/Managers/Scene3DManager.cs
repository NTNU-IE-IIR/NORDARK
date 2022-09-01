using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene3DManager : MonoBehaviour
{
    public const int UNITY_LAYER_TERRAIN = 7;

    private Vector2d mapCenterCoordinates;
    private double mapCenterAltitude;
    double unityUnitsPerLongitude;
    double unityUnitsPerLatitude;
    double unityUnitsPerMeters;
    float worldRelativeScale;

    public void SetLocation(Location location)
    {
        mapCenterCoordinates = location.Coordinates;
        mapCenterAltitude = location.Altitude;
        unityUnitsPerLongitude = location.UnityUnitsPerLongitude;
        unityUnitsPerLatitude = location.UnityUnitsPerLatitude;
        unityUnitsPerMeters = location.UnityUnitsPerMeters;
        worldRelativeScale = location.WorldRelativeScale;
    }

    public Vector3 GetUnityPositionFromCoordinatesAndAltitude(Vector2d latLong, double altitude, bool stickToGround)
    {
        Vector3 position = new Vector3();
        position.x = (float) ((latLong.y - mapCenterCoordinates.y) * unityUnitsPerLongitude);
        position.z = (float) ((latLong.x - mapCenterCoordinates.x) * unityUnitsPerLatitude);
        if (stickToGround) {
            position.y = Terrain.activeTerrain.SampleHeight(position);
        } else {
            position.y = (float) ((altitude - mapCenterAltitude) * unityUnitsPerMeters);
        }
        return position;
    }

    public Vector2d GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return new Vector2d(mapCenterCoordinates.x + position.z / unityUnitsPerLatitude, mapCenterCoordinates.y + position.x / unityUnitsPerLongitude);
    }

    public double GetAltitudeFromUnityPosition(Vector3 position)
    {
        return ((double) position.y) / unityUnitsPerMeters;
    }

    public float GetWorldRelativeScale()
    {
        return worldRelativeScale;
    }
}
