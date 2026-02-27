using Assets.Scripts;
using System.Collections;
using UnityEngine;

using static Assets.Scripts.DragonData;

public class DragonUnit : MonoBehaviour
{

    public string dragonName = "name missing";
    public HealthBar healthBar;
    public Color32 damageColor = new(255, 0, 0, 50);
    public Action QueuedAction { get; private set; }
    public DragonData Data { get; private set; }
    public GameController ControlScript { get; private set; }
    public Movement MoveScript { get; private set; }
    public Vector2Int CurrentPosition { get => MoveScript.CurrentPosition; }
    public bool IsGliding { get => MoveScript.IsGliding; }


    public DragonType dragonType;
    public DragonSize dragonSize;
    public bool isAlly = false;

    [HideInInspector] public bool isSelected;
    [HideInInspector] public bool isAlive;
    [HideInInspector] public int currentHealth;

    public enum Action
    {
        Move,
        Attack,
        Special,
        Wait,
    }

    public void Initialize(GameController controlScript, Movement moveScript)
    {
        ControlScript = controlScript;
        MoveScript = moveScript;

        MoveScript.Initialize(controlScript);
        healthBar.Initialize(this);
        Data = new DragonData(dragonName, dragonType, dragonSize);
        QueuedAction = Action.Wait;
        currentHealth = Data.MaxHealth;
        isSelected = false;
        isAlive = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        healthBar.SetMaxHealth(currentHealth);
        healthBar.SetHealth(currentHealth);
    }

    public void Attack(DragonUnit target)
    {
        target.ReceiveDamage(Data.AttackDamage);
        ControlScript.UpdateInfoCard();
    }

    public void BreathAttack(DragonUnit target)
    {
        target.ReceiveDamage(Data.BreathDamage);
    }

    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            isAlive = false;
            gameObject.SetActive(false);
        }
        else
        {
            int numFlashes = (int)Mathf.Ceil(damage / (DamageValue(DamageScale.Normal) * SizeValue(DragonSize.Tiny)));
            StartCoroutine(DamageAnimation(numFlashes));
        }
    }

    private IEnumerator DamageAnimation(int numFlashes)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        float flashDelay = 0.2f;
        for (int i = 0; i < numFlashes; i++)
        {
            renderer.material.color = damageColor;
            yield return new WaitForSeconds(flashDelay);
            renderer.material.color = Color.white;
            yield return new WaitForSeconds(flashDelay);
        }
    }

    public void Select(bool selectStatus) => isSelected = selectStatus;

    public void NewTurn() => QueuedAction = Action.Move;

    public void SetQueuedAction(Action status) => QueuedAction = status;
}
