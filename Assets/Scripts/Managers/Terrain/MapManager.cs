using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.Map;

public class MapManager : TerrainTypeManager
{
    public const int DEFAULT_ZOOM = 17;
    private const int MAPBOX_MAX_ZOOM = 22;
    private const int MAPBOX_PIXELS_PER_TILE = 256;
    [SerializeField] private TerrainControl terrainControl;
    [SerializeField] private GameObject locationUndefinedWindow;
    public override Location.TerrainType TerrainType { get; set; }
    private AbstractMap map;
    private int numberOfTiles;
    private int numberOfTilesInitialized;
    private List<Tile> tiles;

    void Awake()
    {
        Assert.IsNotNull(sceneManager);
        Assert.IsNotNull(skyManager);
        Assert.IsNotNull(lightComputationManager);
        Assert.IsNotNull(groundTexturesManager);
        Assert.IsNotNull(sceneCamerasManager);
        Assert.IsNotNull(locationsManager);
        Assert.IsNotNull(lightConfigurationsManager);
        Assert.IsNotNull(terrainControl);
        Assert.IsNotNull(locationUndefinedWindow);

        TerrainType = Location.TerrainType.Map;
        map = GetComponent<AbstractMap>();
        map.OnTileFinished += TileFinished;

        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        numberOfTiles = (extentOptions.north + 1 + extentOptions.south) * (extentOptions.west + 1 + extentOptions.east);

        numberOfTilesInitialized = 0;
        tiles = new List<Tile>();
    }

    public override void ChangeLocation(Location location)
    {
        tiles.Clear();
        locationUndefinedWindow.SetActive(location == null);
        
        if (location != null) {
            Mapbox.Utils.Vector2d newCoordinate = new Mapbox.Utils.Vector2d(location.Coordinate.latitude, location.Coordinate.longitude);
            
            if (map.CenterLatitudeLongitude.Equals(newCoordinate) && map.Zoom == location.Zoom) {
                MapLocationChanged();
            } else {
                numberOfTilesInitialized = 0;

                // Buildings are not automatically destroyed, so it has to be done manually
                foreach (Transform tile in transform) {
                    foreach (Transform building in tile) {
                        Destroy(building.gameObject);
                    }
                }
                
                // We initialize the map instead of just updating it because otherwise 
                // some bug occurs with the map
                map.Initialize(newCoordinate, location.Zoom);               
            }
        }
    }

    public override Vector3 GetUnityPositionFromCoordinates(Coordinate coordinates, bool stickToGround = false)
    {
        Vector3 position = Conversions.GeoToWorldPosition(coordinates.latitude, coordinates.longitude, map.CenterMercator, map.WorldRelativeScale).ToVector3xz();

        if (stickToGround) {
            bool isTerrainElevated = map.Terrain.ElevationType == ElevationLayerType.TerrainWithElevation;
            if (isTerrainElevated) {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(position + new Vector3(0, 10000, 0), Vector3.down, out hit, Mathf.Infinity, 1 << TerrainManager.TERRAIN_LAYER)) {
                    position.y = hit.point.y;
                } else {
                    // Fall back to Mapbox elevation if raycast failed
                    position.y = map.QueryElevationInUnityUnitsAt(new Mapbox.Utils.Vector2d(coordinates.latitude, coordinates.longitude));
                }
            } else {
                position.y = 0; 
            }
        } else {
            position.y = (float) coordinates.altitude;
        }
        return position;
    }

    public override Coordinate GetCoordinatesFromUnityPosition(Vector3 position)
    {
        return new Coordinate(position.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale), position.y);
    }

    public override bool IsCoordinateOnMap(Coordinate coordinates)
    {
        Mapbox.Unity.MeshGeneration.Data.UnityTile tile;
		return map.MapVisualizer.ActiveTiles.TryGetValue(Conversions.LatitudeLongitudeToTileId(coordinates.latitude, coordinates.longitude, (int)map.Zoom), out tile);
    }

    public override List<Tile> GetTiles()
    {
        if (tiles.Count == 0) {
            foreach (Transform child in transform) {
                Mapbox.Unity.MeshGeneration.Data.UnityTile tile = child.gameObject.GetComponent<Mapbox.Unity.MeshGeneration.Data.UnityTile>();
                
                if (tile != null) {
                    tiles.Add(new Tile(tile));
                }
            }
        }

        return tiles;
    }

    public override Vector3 GetMapSize()
    {
        List<Tile> tiles = GetTiles();

        float tileSize = 0;
        if (tiles.Count > 0) {
            tileSize = tiles[0].MeshFilter.mesh.bounds.size.x;
        }
        
        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        return tileSize * new Vector3(1 + extentOptions.west + extentOptions.east, 0, 1 + extentOptions.north + extentOptions.south);
    }

    public override string GetGroundFromPosition(Vector3 position)
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(position + new Vector3(0, 10000, 0), Vector3.down, out hit, Mathf.Infinity, 1 << TerrainManager.TERRAIN_LAYER)) {
            string ground = groundTexturesManager.GetPositionTexture(hit.transform.gameObject.GetComponent<Renderer>().material, hit.textureCoord);
            if (ground != "") {
                return ground;
            } 
        }
        return terrainControl.GetGround();
    }

    public override void SetStyle(int styleIndex)
    {
        List<Tile> tiles = GetTiles();
        groundTexturesManager.SetBaseGroundToMaterials(terrainControl.GetGround(), tiles.Select(tile => tile.MeshRenderer.material).ToList());

        if (styleIndex <= (int) ImagerySourceType.MapboxSatelliteStreet) {
            map.ImageLayer.SetLayerSource((ImagerySourceType) styleIndex);
        }
    }

    public override void DisplayBuildings(bool display)
    {
        map.VectorData.GetFeatureSubLayerAtIndex(0).SetActive(display);
    }

    public int GetZoomCoveringSizeAtLatitude(float size, float latitude)
    {
        Mapbox.Unity.Map.RangeTileProviderOptions extentOptions = (Mapbox.Unity.Map.RangeTileProviderOptions) map.Options.extentOptions.GetTileProviderOptions();
        float numberOfTilesOnOneAxis = extentOptions.north + 1 + extentOptions.south;
        int zoom = MAPBOX_MAX_ZOOM;
        float sizeOfZoom = numberOfTilesOnOneAxis * Conversions.GetTileScaleInMeters(latitude, zoom) * MAPBOX_PIXELS_PER_TILE;

        while (zoom > 0 && sizeOfZoom < size) {
            sizeOfZoom = numberOfTilesOnOneAxis * Conversions.GetTileScaleInMeters(latitude, zoom) * MAPBOX_PIXELS_PER_TILE;
            zoom--;
        }

        return zoom;
    }

    private void MapLocationChanged()
    {
        OnLocationChanged();
        SetStyle(terrainControl.GetGroundIndex());
    }

    private void TileFinished(Mapbox.Unity.MeshGeneration.Data.UnityTile tile)
    {
        numberOfTilesInitialized++;

        if (numberOfTilesInitialized == numberOfTiles) {
            MapLocationChanged();
        }        
    }
}