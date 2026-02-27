using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Movement : MonoBehaviour
{
    public GameController ControlScript { get; private set; }
    public DragonUnit DragonScript { get; private set; }
    [HideInInspector] public Vector2Int CurrentPosition;

    public bool IsGliding { get; private set; }
    [SerializeField] Vector2Int startPosition;

    public struct Path
    {
        public List<KeyValuePair<MoveDirection, Vector2Int>> list;

        public Path(Path oldPath)
        {
            list = new(oldPath.list);
        }
        public Path(MoveDirection direction, Vector2Int pos)
        {
            list = new List<KeyValuePair<MoveDirection, Vector2Int>> {
                new KeyValuePair<MoveDirection, Vector2Int>(direction, pos),
            };
        }
        public void Add(MoveDirection direction, Vector2Int position)
            => list.Add(new(direction, position));
        public Vector2Int GetEndPosition()
            => list[^1].Value;
    }
    void Start()
    {
        SetPosition(startPosition);
        IsGliding = false;
    }

    public void Initialize(GameController cntrlScript)
    {
        DragonScript = GetComponent<DragonUnit>();
        ControlScript = cntrlScript;
    }

    public Dictionary<Vector2Int, Path> GetReachablePositions()
        => ExplorePosition(CurrentPosition, DragonScript.Data.Speed);

    private struct Node
    {
        public Vector2Int tile;
        public Path path;
        public int cost;

        public Node(Path path, int cost)
        {
            this.path = path;
            tile = path.GetEndPosition();
            this.cost = cost;
        }
    }

    Dictionary<Vector2Int, Path> ExplorePosition(Vector2Int startPosition, int movement)
    {
        Dictionary<Vector2Int, Node> visited = new();
        HashSet<Vector2Int> restricted = new();
        Queue<Node> queue = new();
        Path firstPath = new(MoveDirection.None, startPosition);
        queue.Enqueue(new Node(firstPath, 0));
        MoveDirection[] directions = {
            MoveDirection.Up,
            MoveDirection.Down,
            MoveDirection.Left,
            MoveDirection.Right,
        };


        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            DragonUnit unit = ControlScript.GetUnitAtPosition(currentNode.tile);

            if (unit != null && unit.isAlive)
            {
                if(unit.isAlly == DragonScript.isAlly)
                {
                    restricted.Add(currentNode.tile);
                }
                else
                {
                    continue;
                }
            }
            else
            {
                visited[currentNode.tile] = currentNode;
            }

            foreach (MoveDirection dir in directions)
            {
                Vector2Int newPos = TranslateDirection(currentNode.tile, dir);
                int newCost = currentNode.cost + GetTileMoveCost(newPos);

                if (visited.ContainsKey(newPos) && visited[newPos].cost <= newCost
                    || newCost > movement)
                {
                    continue;
                }
                else
                {
                    Path newPath = new Path(currentNode.path);
                    newPath.Add(dir, newPos);
                    Node newNode = new(newPath, newCost);
                    queue.Enqueue(newNode);
                }
            }
        }

        Dictionary<Vector2Int, Path> result = new();
        foreach (Node node in visited.Values)
        {
            if (!restricted.Contains(node.tile))
            {
                result.Add(node.tile, node.path);
            }
        }

        return result;
    }

    public void SetPosition(Vector2Int tilePosition)
    {
        ControlScript.MoveTransform(transform, tilePosition);
        CurrentPosition = tilePosition;
    }

    public IEnumerator GlideToPosition(Vector2Int tilePosition)
    {
        IsGliding = true;
        Vector3 start = transform.localPosition;
        Vector3 target = ControlScript.GetCoordinates(tilePosition);
        Vector3 path = target - start;
        for (int i = 1; i <= 16; i++)
        {
            transform.localPosition = start + (path * i / 16);
            yield return new WaitForSeconds(0.1f / Mathf.Pow(DragonScript.Data.Speed, 2));
        }
        CurrentPosition = tilePosition;
        IsGliding = false;
    }

    public Vector2Int TranslateDirection(Vector2Int pos, MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Up:
                return pos + Vector2Int.up;
            case MoveDirection.Down:
                return pos + Vector2Int.down;
            case MoveDirection.Left:
                return pos + Vector2Int.left;
            case MoveDirection.Right:
                return pos + Vector2Int.right;
            default:
                return pos;
        }
    }

    public Vector2Int TranslateDirection(MoveDirection dir) 
        => TranslateDirection(CurrentPosition, dir);

    public int GetTileMoveCost(Vector2Int pos) => ControlScript.GetTileMoveCost(pos);
}
