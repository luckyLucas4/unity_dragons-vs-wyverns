using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static DragonUnit;
using static Assets.Scripts.DragonData;
using Assets.Scripts;
using System.Collections;

public class AllyInteraction : MonoBehaviour
{
    public CooldownBar cooldownBar;
    public GameObject firePrefab;
    public GameController ControlScript { get; private set; }

    public Movement MoveScript { get; private set; }
    public Vector2Int CurrentPosition { get => MoveScript.CurrentPosition; }
    public bool IsGliding { get => MoveScript.IsGliding; }

    public DragonUnit DragonScript { get; private set; }
    public Action QueuedAction { get => DragonScript.QueuedAction; }
    public DragonData Data { get => DragonScript.Data; }
    public bool IsAlive { get => DragonScript.isAlive; }

    Animator animator;
    bool isAnimating;
    Queue<MoveDirection> plannedPath;
    int currentCooldown;

    public void Initialize(GameController cntrlScript)
    {
        ControlScript = cntrlScript;
        MoveScript = GetComponent<Movement>();
        DragonScript = GetComponent<DragonUnit>();
        animator = GetComponent<Animator>();

        DragonScript.Initialize(cntrlScript, MoveScript);
        DragonScript.isAlly = true;

        currentCooldown = 0;
        isAnimating = true;
        plannedPath = new();
    }

    void Start()
    {
        cooldownBar.SetMaxCooldown(Data.SpecialCooldown);
        cooldownBar.SetCooldown(currentCooldown);
    }

    private void Update()
    {
        if (isAnimating && QueuedAction == Action.Wait)
        {
            animator.enabled = false;
            isAnimating = false;
        }
        else if (!isAnimating && QueuedAction != Action.Wait)
        {
            animator.enabled = true;
            isAnimating = true;
        }

        if (plannedPath.Count == 0)
        {
            ControlScript.repaintIndicators = true;
        }
        if (plannedPath.Count > 0 && !IsGliding)
        {
            ControlScript.ClearIndicators();
            Vector2Int newPos = MoveScript.TranslateDirection(plannedPath.Dequeue());
            StartCoroutine(MoveScript.GlideToPosition(newPos));
        }
    }

    public bool HandleInput(Vector2Int mousePosition)
    {
        // Return true if finished

        if (IsGliding)
            return false;

        switch (QueuedAction)
        {
            case Action.Move:
                return MovementInput(mousePosition);
            case Action.Attack:
                return AttackInput(mousePosition);
            case Action.Special:
                return SpecialInput(mousePosition);
            case Action.Wait:
                break;
        }
        return true;
    }

    private bool MovementInput(Vector2Int mousePosition)
    {
        if (CurrentPosition == mousePosition)
        {
            DragonScript.SetQueuedAction(Action.Attack);
            return false;
        }
        else
        {
            var reachPositions = MoveScript.GetReachablePositions();
            if (reachPositions.ContainsKey(mousePosition))
            {
                Movement.Path path = reachPositions[mousePosition];
                path.list.ForEach(item => plannedPath.Enqueue(item.Key));
                DragonScript.SetQueuedAction(Action.Attack);
                return false;
            }
            else
                return true;
        }
    }

    private bool AttackInput(Vector2Int mousePosition)
    {
        if (CurrentPosition == mousePosition)
        {
            if (Data.Type == DragonType.Fire && currentCooldown == 0)
            {
                DragonScript.SetQueuedAction(Action.Special);
                return false;
            }
            else
            {
                DragonScript.SetQueuedAction(Action.Wait);
            }
        }
        else if (GetAttackablePositions().Contains(mousePosition))
        {
            DragonScript.Attack(
                ControlScript.GetUnitAtPosition(mousePosition)
                );

            if (currentCooldown == 0)
            {
                DragonScript.SetQueuedAction(Action.Special);
                return false;
            }
            else
            {
                DragonScript.SetQueuedAction(Action.Wait);
            }
        }
        return true;
    }

    private bool SpecialInput(Vector2Int mousePosition)
    {
        if (CurrentPosition == mousePosition || currentCooldown > 0)
        {
            DragonScript.SetQueuedAction(Action.Wait);
            return true;
        }
        switch (Data.Type)
        {
            case DragonType.Plain:
                if (GetAttackablePositions().Contains(mousePosition))
                {
                    ResetCooldown();
                    DragonScript.SetQueuedAction(Action.Wait);
                    DragonScript.Attack(ControlScript.GetUnitAtPosition(mousePosition));
                }
                break;
            case DragonType.Ice:
                var reachablePositions = MoveScript.GetReachablePositions();
                if (reachablePositions.ContainsKey(mousePosition))
                {
                    ResetCooldown();
                    DragonScript.SetQueuedAction(Action.Wait);
                    Movement.Path path = reachablePositions[mousePosition];
                    path.list.ForEach(item => plannedPath.Enqueue(item.Key));
                }
                break;
            case DragonType.Fire:
                var breathLines = GetBreathPositions(Data.BreathRange);
                foreach (List<Vector2Int> line in breathLines)
                {
                    if (line.Contains(mousePosition))
                    {
                        ResetCooldown();
                        DragonScript.SetQueuedAction(Action.Wait);
                        foreach (Vector2Int pos in line)
                        {
                            DragonUnit unit = ControlScript.GetUnitAtPosition(pos);
                            if (unit != null)
                            {
                                DragonScript.BreathAttack(unit);
                            }
                        }
                        StartCoroutine(FireEffect(line));
                    }
                }
                break;
            default:
                break;
        }
        return true;
    }

    public void AddMovementPlan(Movement.Path path)
    {
        if (plannedPath.Count == 0)
        {
            path.list.ForEach(item => plannedPath.Enqueue(item.Key));
            DragonScript.SetQueuedAction(Action.Attack);
        }
    }

    public HashSet<Vector2Int> GetAttackablePositions()
    {
        HashSet<Vector2Int> result = new();
        MoveDirection[] directions = {
            MoveDirection.Up,
            MoveDirection.Down,
            MoveDirection.Left,
            MoveDirection.Right,
        };
        foreach (MoveDirection dir in directions)
        {
            Vector2Int newPos = MoveScript.TranslateDirection(dir);
            DragonUnit unit = ControlScript.GetUnitAtPosition(newPos);
            if (unit != null && !unit.isAlly && unit.isAlive)
            {
                result.Add(newPos);
            }
        }
        return result;
    }

    public List<List<Vector2Int>> GetBreathPositions(int range)
    {
        List<List<Vector2Int>> result = new();
        MoveDirection[] directions = {
            MoveDirection.Up,
            MoveDirection.Down,
            MoveDirection.Left,
            MoveDirection.Right,
        };
        foreach (MoveDirection dir in directions)
        {
            List<Vector2Int> line = new();
            Vector2Int newPos = CurrentPosition;
            for (int i = 0; i < range; i++)
            {
                newPos = MoveScript.TranslateDirection(newPos, dir);
                if (ControlScript.InBounds(newPos))
                {
                    line.Add(newPos);
                }
            }
            result.Add(line);
        }
        return result;
    }
    public void ResetCooldown()
    {
        currentCooldown = Data.SpecialCooldown;
        cooldownBar.SetCooldown(currentCooldown);
    }

    public void NewTurn()
    {
        DragonScript.NewTurn();
        if (currentCooldown > 0)
        {
            currentCooldown--;
            cooldownBar.SetCooldown(currentCooldown);
        }
    }


    public IEnumerator FireEffect(List<Vector2Int> positions)
    {
        if (firePrefab == null) 
        {
            yield break;
        }
        List<GameObject> fires= new();
        foreach (Vector2Int position in positions)
        {
            Vector3 coords = ControlScript.GetCoordinates(position);
            coords.z = ControlScript.effectsLayer.transform.position.z;
            fires.Add(
                Instantiate(
                    firePrefab,
                    coords,
                    Quaternion.identity,
                    ControlScript.effectsLayer
                    )
                );
        }
        yield return new WaitForSeconds(2);
        foreach (GameObject fire in fires)
        {
            Destroy(fire);
        }
    }
}
