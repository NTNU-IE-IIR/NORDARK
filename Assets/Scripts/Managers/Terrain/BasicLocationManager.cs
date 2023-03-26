using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BasicLocationManager : TerrainTypeManager
{
    public const float DEFAULT_CAMERA_HEIGHT = 50;
    public const float DEFAULT_CAMERA_ANGLE_X = 60;
    [SerializeField] private TerrainControl terrainControl;
    public override Location.TerrainType TerrainType { get; set; }
    private Material material;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(skyManager);
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(terrainControl);

        TerrainType = Location.TerrainType.Basic;
        material = GetComponent<Renderer>().material;
    }

    public override void ChangeLocation(Location location)
    {
        OnLocationChanged();
    }

    public override Vector3 GetUnityPositionFromCoordinates(Coordinate coordinates, bool stickToGround = false)
    {
        return new Vector3((float) coordinates.latitude, (float) coordinates.altitude, (float) coordinates.longitude);
    }

    public override Coordinate GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return new Coordinate(position.x, position.z);
    }

    public override bool IsCoordinateOnMap(Coordinate coordinates)
    {
		return
            coordinates.latitude < transform.localScale.x / 2 && coordinates.latitude > -transform.localScale.x / 2 &&
            coordinates.longitude < transform.localScale.y / 2 && coordinates.longitude > -transform.localScale.y / 2
        ;
    }

    public override List<Tile> GetTiles()
    {
        return new List<Tile>();
    }

    public override Vector3 GetMapSize()
    {
        return new Vector3(transform.localScale.x, 0, transform.localScale.y);
    }

    public override string GetGroundFromPosition(Vector3 position)
    {
        return terrainControl.GetGround();
    }

    public override void SetStyle(int styleIndex)
    {
        groundTexturesManager.SetBaseGroundToMaterials(terrainControl.GetGround(), new List<Material>{ material });
    }

    public override void DisplayBuildings(bool display)
    {}
}
