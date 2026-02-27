using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static DragonUnit;
using static Assets.Scripts.MyTileData;

public class EnemyInteraction : MonoBehaviour
{
    public Queue<MoveDirection> plannedSteps = new();
    public GameController ControlScript { get; private set; }

    public Movement MoveScript { get; private set; }
    public bool IsGliding { get => MoveScript.IsGliding; }
    public Vector2Int CurrentPosition { get => MoveScript.CurrentPosition; }

    public DragonUnit DragonScript { get; private set; }
    public Action QueuedAction { get => DragonScript.QueuedAction; }
    public DragonData Data { get => DragonScript.Data; }
    public bool IsAlive { get => DragonScript.isAlive; }

    DragonUnit target;
    int moveSpeedRemaining = 0;
    int moveAttemptsRemaining = 0;

    public void Initialize(GameController cntrlScript)
    {
        ControlScript = cntrlScript;
        MoveScript = GetComponent<Movement>();
        DragonScript = GetComponent<DragonUnit>();

        DragonScript.Initialize(cntrlScript, MoveScript);
    }
    public void NewTurn()
        => DragonScript.NewTurn();

    public bool Continue()
    {
        // Returns true if finished
        if (IsGliding)
            return false;
        switch (QueuedAction)
        {
            case Action.Move:
                FindTarget();
                moveSpeedRemaining = Data.Speed;
                DragonScript.SetQueuedAction(Action.Attack);
                moveAttemptsRemaining = 100;
                return false;
            case Action.Attack:
                moveAttemptsRemaining--;
                if (moveSpeedRemaining < 0 || moveAttemptsRemaining < 0 || !AttemptStep())
                {
                    DragonScript.SetQueuedAction(Action.Wait);
                    return true;
                }
                else
                    return false;
            default:
                return true;
        }
    }

    void FindTarget()
    {
        HashSet<Vector2Int> exploredPositions = new();
        Queue<Movement.Path> paths = new();

        paths.Enqueue(new(MoveDirection.None, CurrentPosition));
        while (paths.Count > 0)
        {
            Movement.Path path = paths.Dequeue();
            Vector2Int currentPos = path.list.Last().Value;

            MoveDirection[] directions = {
                MoveDirection.Up,
                MoveDirection.Down,
                MoveDirection.Left,
                MoveDirection.Right,
            };
            foreach (MoveDirection dir in directions)
            {
                Vector2Int newPos = MoveScript.TranslateDirection(currentPos, dir);
                DragonUnit unit = ControlScript.GetUnitAtPosition(newPos);

                Movement.Path newPath = new(path);
                if (unit != null && unit.isAlly && !unit.IsGliding)
                {
                    newPath.list.Add(new(dir, newPos)); 
                    plannedSteps.Clear();
                    foreach (var step in newPath.list)
                    {
                        plannedSteps.Enqueue(step.Key);
                    }
                    target = unit;
                    return;
                }
                else if (unit != null && !unit.isAlly)
                {
                    exploredPositions.Add(newPos);
                }
                else if (MoveScript.GetTileMoveCost(newPos) == (int)MoveCostType.Stop 
                    || exploredPositions.Contains(newPos))
                    continue;
                else
                {
                    exploredPositions.Add(newPos);
                    newPath.list.Add(new(dir, newPos));
                    paths.Enqueue(newPath);
                }
            }
        }
    }

    bool AttemptStep()
    {
        if (plannedSteps.Count == 0)
            return false;
        MoveDirection currentStep = plannedSteps.Dequeue();

        if (currentStep == MoveDirection.None)
            return true;

        Vector2Int plannedPos = MoveScript.TranslateDirection(CurrentPosition, currentStep);

        if (plannedSteps.Count == 0)
        {
            if (target.CurrentPosition == plannedPos)
            {
                DragonScript.Attack(target);

            }
            return false;
        }
        else
        {
            if(ControlScript.GetUnitAtPosition(plannedPos) != null)
            {
                return false;
            }
            moveSpeedRemaining -= ControlScript.GetTileMoveCost(plannedPos);
            if (moveSpeedRemaining < 0)
                return false;
            else
            {
                StartCoroutine(MoveScript.GlideToPosition(plannedPos));
                return true;
            }
        }
    }
}

