using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Assets.Scripts.MyTileData;
using static Assets.Scripts.ObstacleData;
using static GameController;

namespace Assets.Scripts
{
    public class BoardHandler
    {
        public abstract class Board
        {
            public List<DragonUnit> Units { get; private set; } = new();
            public Dictionary<Vector2Int, Obstacle> Obstacles { get; private set; } = new();

            public readonly string name;
            protected readonly Vector2Int dim;
            protected GameController cntrlScript;
            protected GameObject obstacleLayer;
            protected Tilemap terrainMap;
            protected Tilemap indicatorMap;
            protected List<List<MyTileData>> board;

            public Board(
                string name,
                Vector2Int dim,
                GameController cntrlScript,
                Tilemap terrainMap,
                Tilemap indicatorMap
                )
            {
                this.name = name;
                this.dim = dim;
                this.cntrlScript = cntrlScript;
                this.terrainMap = terrainMap;
                this.indicatorMap = indicatorMap;
            }

            protected abstract List<List<MyTileData>> CreateBoard();

            public MyTileData this[int x, int y]
            {
                get => board[x][y];
            }
            public MyTileData this[Vector2Int pos]
            {
                get => board[pos.x][pos.y];
            }

            public bool InBounds(int x, int y)
                => (x >= 0 && x < dim.x && y >= 0 && y < dim.y);
            public bool InBounds(Vector2Int pos) => InBounds(pos.x, pos.y);

            public void AddUnit(DragonUnit unit)
            {
                if (unit == null)
                    throw new ArgumentNullException(nameof(unit));

                if (!InBounds(unit.MoveScript.CurrentPosition))
                {
                    throw new ArgumentOutOfRangeException(nameof(unit));
                }

                Units.Add(unit);
            }

            public void RemoveUnit(DragonUnit unit)
            {
                Units.Remove(unit);
            }

            public void AddObstacle(ObstacleType obstacleType, Vector2Int position)
            {

                if (!InBounds(position) || this[position].tileType == TileType.Empty)
                {
                    Debug.Log("Tried to place obstacle " + obstacleType + " at empty position: " + position);
                    return;
                }

                Obstacle obstacle = cntrlScript.GetObstacleFromObstacleType(obstacleType);
                cntrlScript.CreateObstacle(obstacle, position);
                obstacle.Initialize(this[position].tileType);
                Obstacles[position] = obstacle;
                obstacle.position = position;
            }

            public void PaintBoard()
            {
                terrainMap.ClearAllTiles();
                for (int x = 0; x < dim.x; x++)
                {
                    for (int y = 0; y < dim.y; y++)
                    {
                        Tile tile = cntrlScript.GetTileFromTileType(this[x, y].tileType);
                        terrainMap.SetTile(new Vector3Int(x, y), tile);
                    }
                }
            }

            public void PaintIndicators(HashSet<Vector2Int> positions, Tile indicator)
            {
                foreach (Vector2Int pos in positions)
                {
                    indicatorMap.SetTile(new Vector3Int(pos.x, pos.y), indicator);
                }
            }
            public void PaintIndicator(Vector2Int position, Tile indicator)
            {
                indicatorMap.SetTile(new Vector3Int(position.x, position.y), indicator);
            }

            public int GetTileMoveCost(Vector2Int pos)
            {
                if (InBounds(pos) && GetObstacleAtPosition(pos) == null)
                {
                    return this[pos].MoveCost;
                }
                else
                {
                    return (int)MoveCostType.Stop;
                }
            }

            public DragonUnit GetUnitAtPosition(Vector2Int pos)
            {
                foreach (DragonUnit unit in Units)
                {
                    if (unit.MoveScript.CurrentPosition == pos)
                        return unit;
                }
                return null;
            }

            public Obstacle GetObstacleAtPosition(Vector2Int pos)
            {
                if (Obstacles.Keys.Contains(pos))
                {
                    return Obstacles[pos];
                }
                return null;
            }
        }

        public class CodedBoard : Board
        {
            public TileType baseTile { get; }

            List<BoardSection> sections;
            public CodedBoard(
                string name,
                Vector2Int dim,
                TileType baseTile,
                List<BoardSection> sections,
                Dictionary<Vector2Int, ObstacleType> obstacles,
                GameController cntrl,
                Tilemap terrainMap,
                Tilemap indicatorMap
                )
                : base(name, dim, cntrl, terrainMap, indicatorMap)
            {
                this.baseTile = baseTile;
                this.sections = sections;
                this.board = CreateBoard();
                foreach (KeyValuePair<Vector2Int, ObstacleType> kvp in obstacles)
                {
                    AddObstacle(kvp.Value, kvp.Key);
                }
            }

            protected override List<List<MyTileData>> CreateBoard()
            {
                List<List<MyTileData>> newBoard = new();
                List<MyTileData> baseColumn = Enumerable.Repeat(new MyTileData(baseTile), dim.y).ToList();
                for (int i = 0; i < dim.x; i++)
                {
                    newBoard.Add(new List<MyTileData>(baseColumn));
                }

                foreach (BoardSection section in sections)
                {
                    int posX = section.pos.x;
                    int posY = section.pos.y;
                    for (int x = 0; x < section.size.x; x++)
                    {
                        for (int y = 0; y < section.size.y; y++)
                        {
                            TileType currentTile = newBoard[posX + x][posY + y].tileType;
                            if (currentTile == baseTile)
                            {
                                newBoard[posX + x][posY + y] = new MyTileData(section.tileType);
                            }
                            else
                            {
                                Debug.Log("Tried to overwrite tile type " + currentTile + " with " + section.tileType);
                            }
                        }
                    }
                }
                return newBoard;
            }
        }

        public class BoardSection
        {
            public Vector2Int pos;
            public Vector2Int size;
            public TileType tileType;

            public BoardSection(Vector2Int pos, Vector2Int size, TileType tileType)
            {
                this.pos = pos;
                this.size = size;
                this.tileType = tileType;
            }

            public BoardSection(int posX, int posY, int sizeX, int sizeY,
                TileType tileType)
            {
                this.pos = new Vector2Int(posX, posY);
                this.size = new Vector2Int(sizeX, sizeY);
                this.tileType = tileType;
            }
        }

        void FillObstacleSection(Vector2Int pos, Vector2Int size, ObstacleType obstacle, Dictionary<Vector2Int, ObstacleType> obstacles)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    obstacles[pos + new Vector2Int(x, y)] = obstacle;
                }
            }
        }

        public CodedBoard InitializeBoard(
            Level level,
            GameController cntrlScript,
            Tilemap terrainMap,
            Tilemap indicatorMap
            )
        {
            string name;
            Vector2Int dim;
            TileType baseTile;
            List<BoardSection> sections;
            Dictionary<Vector2Int, ObstacleType> obstacles = new();

            switch (level)
            {
                case Level.StartingGrounds:
                    name = "Starting grounds";
                    dim = new Vector2Int(5, 5);
                    baseTile = TileType.Plains;
                    sections = new()
                    {

                    };
                    obstacles[new Vector2Int(2, 3)] = ObstacleType.Tree;
                    break;
                case Level.NextingGrounds:
                    name = "Nexting grounds";
                    dim = new Vector2Int(10, 10);
                    baseTile = TileType.Plains;
                    sections = new()
                    {
                        new BoardSection(new Vector2Int(6,4), new Vector2Int(3,3), TileType.Mud),
                        new BoardSection(new Vector2Int(0, 6), new Vector2Int(6, 1), TileType.Empty)
                    };
                    obstacles[new Vector2Int(6, 6)] = ObstacleType.Tree;
                    obstacles[new Vector2Int(7, 6)] = ObstacleType.Tree;
                    obstacles[new Vector2Int(3, 7)] = ObstacleType.Tree;
                    FillObstacleSection(new Vector2Int(0, 2), new Vector2Int(4, 1), ObstacleType.Mountain, obstacles);
                    break;
                case Level.PointChoke:
                    name = "Point Choke";
                    dim = new Vector2Int(10, 5);
                    baseTile = TileType.Plains;
                    sections = new()
                    {
                        new BoardSection(new Vector2Int(0,0), new Vector2Int(2,5), TileType.Ice),
                    };
                    obstacles[new Vector2Int(5, 1)] = ObstacleType.Mountain;
                    FillObstacleSection(new Vector2Int(3, 0), new Vector2Int(3, 1), ObstacleType.Mountain, obstacles);
                    FillObstacleSection(new Vector2Int(4, 3), new Vector2Int(2, 2), ObstacleType.Mountain, obstacles);
                    break;
                case Level.Barrier:
                    name = "Barrier";
                    dim = new Vector2Int(9, 9);
                    baseTile = TileType.Sand;
                    sections = new()
                    {
                        new BoardSection(new Vector2Int(6,3), new Vector2Int(2,3), TileType.Mud),
                    };
                    obstacles[new Vector2Int(0, 8)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(1, 7)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(2, 6)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(8, 0)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(7, 1)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(6, 2)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(3, 3)] = ObstacleType.Mountain;
                    obstacles[new Vector2Int(5, 5)] = ObstacleType.Mountain;
                    FillObstacleSection(new Vector2Int(3, 4), new Vector2Int(3, 1), ObstacleType.Mountain, obstacles);
                    break;
                case Level.Terrain:
                    name = "Terrain";
                    dim = new Vector2Int(12, 8);
                    baseTile = TileType.Plains;
                    sections = new()
                    {
                        new BoardSection(new Vector2Int(2,0), new Vector2Int(8,1), TileType.Mud),
                        new BoardSection(new Vector2Int(2,2), new Vector2Int(8,1), TileType.Stone),
                        new BoardSection(new Vector2Int(2,5), new Vector2Int(8,1), TileType.Sand),
                        new BoardSection(new Vector2Int(2,7), new Vector2Int(8,1), TileType.Ice),
                        new BoardSection(new Vector2Int(2,1), new Vector2Int(8,1), TileType.Empty),
                        new BoardSection(new Vector2Int(2,3), new Vector2Int(8,2), TileType.Empty),
                        new BoardSection(new Vector2Int(2,6), new Vector2Int(8,1), TileType.Empty),
                    };
                    break;
                default:
                    return null;
            }
            return new CodedBoard(name, dim, baseTile, sections, obstacles, cntrlScript, terrainMap, indicatorMap);
        }
    }
}
