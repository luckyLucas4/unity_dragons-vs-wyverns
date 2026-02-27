using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Assets.Scripts.MyTileData;
using static Assets.Scripts.ObstacleData;

public class MyResources : MonoBehaviour
{
    [Header("Tiles")]
    public Tile tileWalkIndicator;
    public Tile tileAttackIndicator;
    public Tile tileNeutralIndicator;
    public Tile tilePlain, tileMud, tileStone, tileSand, tileIce;

    [Header("Obstacles")]
    public Obstacle tree;
    public Obstacle mountain;
    public Obstacle house;

    bool initialized = false;

    Dictionary<Tile, TileType> tileTranslation;
    Dictionary<TileType, Tile> tileTranslationReversed;

    Dictionary<Obstacle, ObstacleType> obstacleTranslation;
    Dictionary<ObstacleType, Obstacle> obstacleTranslationReversed;

    void Initialize()
    {
        tileTranslation = new Dictionary<Tile, TileType>()
        {
            {tilePlain, TileType.Plains},
            {tileMud, TileType.Mud},
            {tileStone, TileType.Stone},
            {tileSand, TileType.Sand},
            {tileIce, TileType.Ice},
        };
        tileTranslationReversed = new();
        foreach (KeyValuePair<Tile, TileType> kvp in tileTranslation)
        {
            tileTranslationReversed.Add(kvp.Value, kvp.Key);
        }

        obstacleTranslation = new()
        {
            {tree, ObstacleType.Tree},
            {mountain, ObstacleType.Mountain},
            {house, ObstacleType.House},
        };
        obstacleTranslationReversed = new();
        foreach (KeyValuePair<Obstacle, ObstacleType> kvp in obstacleTranslation)
        {
            obstacleTranslationReversed.Add(kvp.Value, kvp.Key);
        }
        initialized = true;
    }

    public TileType GetTileTypeFromTile(Tile tile)
    {
        if (!initialized)
            Initialize();
        return tileTranslation[tile];
    }
    public Tile GetTileFromTileType(TileType tileType)
    {
        if (!initialized)
            Initialize();
        if (tileType == TileType.Empty)
            return null;
        return tileTranslationReversed[tileType];
    }

    public ObstacleType GetObstacleTypeFromObstacle(Obstacle obstacle)
    {
        if (!initialized)
            Initialize();
        return obstacleTranslation[obstacle];
    }
    public Obstacle GetObstacleFromObstacleType(ObstacleType obstacleType)
    {
        if (!initialized)
            Initialize();
        return obstacleTranslationReversed[obstacleType];
    }
}
