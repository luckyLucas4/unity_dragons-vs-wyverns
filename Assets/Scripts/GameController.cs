using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Assets.Scripts.MyTileData;
using static Assets.Scripts.ObstacleData;
using static Assets.Scripts.DragonData;
using static DragonUnit;

public class GameController : MonoBehaviour
{
    [HideInInspector] public bool repaintIndicators;

    public Transform effectsLayer;
    [SerializeField] Grid grid;
    [SerializeField] Tilemap terrainMap;
    [SerializeField] Tilemap indicatorMap;
    [SerializeField] Transform obstacleLayer;
    [SerializeField] GameInterface gameInterface;
    [SerializeField] List<AllyInteraction> allies;
    [SerializeField] List<EnemyInteraction> enemies;
    [SerializeField] Level currentLevel;

    MyResources resourcesScript;
    readonly BoardHandler boardsScript = new();
    BoardHandler.Board currentBoard;
    AllyInteraction selectedAlly;
    EnemyInteraction selectedEnemy;
    bool isPlayerTurn;
    bool isPaused;

    public enum Level
    {
        StartingGrounds = 1,
        NextingGrounds = 2,
        Terrain = 3,
        PointChoke = 4,
        Barrier = 5,
    }
    void Awake()
    {
        resourcesScript = GetComponent<MyResources>();
        allies.ForEach((ally) => ally.Initialize(this));
        enemies.ForEach((enemy) => enemy.Initialize(this));
    }

    // Start is called before the first frame update
    void Start()
    {
        repaintIndicators = false;
        isPlayerTurn = true;
        isPaused = false;

        BoardHandler.CodedBoard newBoard = boardsScript.InitializeBoard(
            currentLevel, this, terrainMap, indicatorMap
            );
        if (newBoard != null)
        {
            currentBoard = newBoard;
            currentBoard.PaintBoard();
        }
        foreach (AllyInteraction ally in allies)
        {
            currentBoard.AddUnit(ally.DragonScript);
            ally.NewTurn();
        }
        foreach (EnemyInteraction enemy in enemies)
        {
            currentBoard.AddUnit(enemy.DragonScript);
            enemy.NewTurn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckDeaths();
        Vector2Int mousePosition = GetMousePosition();
        if (Input.GetMouseButtonDown(1))
            HandleInfoCard(mousePosition);

        if (isPaused)
            return;

        if (isPlayerTurn)
        {
            if (Input.GetMouseButtonDown(0))
                HandleSelection(mousePosition);

            if (repaintIndicators 
                && selectedAlly != null 
                && !selectedAlly.MoveScript.IsGliding
                )
                PaintAllyIndicators(selectedAlly);

        }
        else
        {
            if (selectedEnemy == null)
            {
                foreach (EnemyInteraction enemy in enemies)
                {
                    if (!enemy.Continue())
                    {
                        // If the enemy is not finished, assign it as selected
                        selectedEnemy = enemy;
                        break;
                    }
                }
            }
            else
            {
                // If the selected enemy is finished, reset selectedEnemy
                if (selectedEnemy.Continue())
                    selectedEnemy = null;
            }
        }

        StartCoroutine(CheckTurnEnd());
    }

    IEnumerator CheckTurnEnd()
    {
        if (isPlayerTurn)
        {
            if (allies.TrueForAll(item => item.QueuedAction == Action.Wait))
            {
                selectedAlly = null;
                gameInterface.UpdateTurnLabel(false);
                isPaused = true;
                yield return new WaitForSeconds(1);
                isPaused = false;
                enemies.ForEach(enemy => enemy.NewTurn());
                isPlayerTurn = false;
            }
        }
        else
        {
            if (enemies.TrueForAll(item => item.QueuedAction == Action.Wait))
            {
                isPaused = true;
                yield return new WaitForSeconds(1);
                isPaused = false;
                selectedEnemy = null;
                gameInterface.UpdateTurnLabel(true);
                allies.ForEach((ally) => ally.NewTurn());
                isPlayerTurn = true;

            }
        }
        yield break;
    }

    void HandleInfoCard(Vector2Int mousePosition)
    {
        DragonUnit selectedDragon = null;
        foreach (AllyInteraction ally in allies)
        {
            if (mousePosition == ally.CurrentPosition)
            {
                selectedDragon = ally.DragonScript;
                break;
            }
        }
        if (selectedDragon == null)
        {
            foreach (EnemyInteraction enemy in enemies)
            {
                if (mousePosition == enemy.CurrentPosition)
                {
                    selectedDragon = enemy.DragonScript;
                    break;
                }
            }
        }

        if (selectedDragon != null)
        {
            gameInterface.ShowInfoCard(selectedDragon, selectedDragon.isAlly);
        }
        else if (gameInterface.InfoCardShowing)
        {
            gameInterface.HideInfoCard();
        }
    }

    public void UpdateInfoCard()
    {
        gameInterface.UpdateInfoCard();
    }

    void HandleSelection(Vector2Int mousePosition)
    {
        ClearIndicators();
        repaintIndicators = true;
        if (selectedAlly == null)
        {
            foreach (AllyInteraction ally in allies)
            {
                if (mousePosition == ally.CurrentPosition)
                {
                    if (ally.QueuedAction != Action.Wait)
                    {
                        selectedAlly = ally;
                        selectedAlly.DragonScript.Select(true);
                    }
                    break;
                }
            }
        }
        else
        {
            bool finished = selectedAlly.HandleInput(mousePosition);
            if(finished)
            {
                selectedAlly.DragonScript.Select(false);
                selectedAlly = null;
                repaintIndicators = false;
            }
        }
    }

    void PaintAllyIndicators(AllyInteraction ally)
    {
        repaintIndicators = false;
        currentBoard.PaintIndicator(ally.CurrentPosition, resourcesScript.tileNeutralIndicator);

        switch (ally.QueuedAction)
        {
            case Action.Wait:
                break;
            case Action.Move:
                currentBoard.PaintIndicators(
                    ally.MoveScript.GetReachablePositions().Keys.ToHashSet(), 
                    resourcesScript.tileWalkIndicator
                    );
                break;
            case Action.Attack:
                currentBoard.PaintIndicators(
                    ally.GetAttackablePositions(), 
                    resourcesScript.tileAttackIndicator
                    );
                break;
            case Action.Special:
                switch (selectedAlly.Data.Type)
                {
                    case DragonType.Plain:
                        currentBoard.PaintIndicators(
                            ally.GetAttackablePositions(),
                            resourcesScript.tileAttackIndicator
                            );
                        break;
                    case DragonType.Ice:
                        currentBoard.PaintIndicators(
                            ally.MoveScript.GetReachablePositions().Keys.ToHashSet(),
                            resourcesScript.tileWalkIndicator
                            );
                        break;
                    case DragonType.Fire:
                        HashSet<Vector2Int> tiles = new();
                        var breathLines = ally.GetBreathPositions(ally.Data.BreathRange);
                        foreach (List<Vector2Int> line in breathLines)
                        {
                            tiles.UnionWith(line);
                        }
                        currentBoard.PaintIndicators(
                            tiles, 
                            resourcesScript.tileAttackIndicator
                            );
                        break;
                }
                break;
        }
    }

    void CheckDeaths()
    {
        foreach (AllyInteraction ally in allies.ToList())
        {
            if (!ally.IsAlive)
            {
                allies.Remove(ally);
                currentBoard.RemoveUnit(ally.DragonScript);
            }
        }
        if (allies.Count == 0)
        {
            gameInterface.ShowEndScreen(false);
            return;
        }

        foreach (EnemyInteraction enemy in enemies.ToList())
        {
            if (!enemy.IsAlive)
            {
                enemies.Remove(enemy);
                currentBoard.RemoveUnit(enemy.DragonScript);
            }
        }
        if (enemies.Count == 0)
        {
            gameInterface.ShowEndScreen(true);
            return;
        }
    }

    public int GetTileMoveCost(Vector2Int pos)
    {
        return currentBoard.GetTileMoveCost(pos);
    }

    public DragonUnit GetUnitAtPosition(Vector2Int pos)
    {
        foreach (AllyInteraction ally in allies)
        {
            if (ally.IsAlive && ally.CurrentPosition == pos)
            {
                return ally.DragonScript;
            }
        }
        foreach (EnemyInteraction enemy in enemies)
        {
            if (enemy.IsAlive && enemy.CurrentPosition == pos)
            {
                return enemy.DragonScript;
            }
        }
        return null;
    }

    public void CreateObstacle(Obstacle obstacle, Vector2Int position)
    {
        Instantiate(
            obstacle.gameObject, 
            GetCoordinates(position), 
            Quaternion.identity, 
            obstacleLayer.transform
            );
    }

    public TileType GetTileTypeFromTile(Tile tile)
        => resourcesScript.GetTileTypeFromTile(tile);
    public Tile GetTileFromTileType(TileType tileType)
        => resourcesScript.GetTileFromTileType(tileType);
    public ObstacleType GetObstacleTypeFromObstacle(Obstacle obstacle)
        => resourcesScript.GetObstacleTypeFromObstacle(obstacle);
    public Obstacle GetObstacleFromObstacleType(ObstacleType obstacleType)
        => resourcesScript.GetObstacleFromObstacleType(obstacleType);

    public bool InBounds(Vector2Int pos)
        => currentBoard.InBounds(pos);

    public void MoveTransform(Transform transform, Vector2Int pos)
    {
        Vector3Int cellPosition = grid.LocalToCell(transform.localPosition);
        cellPosition.x = pos.x;
        cellPosition.y = pos.y;
        transform.localPosition = grid.CellToLocal(cellPosition);
    }

    public Vector3 GetCoordinates(Vector2Int position) 
        => grid.CellToLocal(new Vector3Int(position.x, position.y));

    Vector2Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = grid.WorldToCell(mouseWorldPos);
        return new Vector2Int(gridPosition.x, gridPosition.y);
    }

    public void ClearIndicators() => indicatorMap.ClearAllTiles();
}
