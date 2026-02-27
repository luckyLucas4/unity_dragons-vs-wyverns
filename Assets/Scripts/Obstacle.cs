using Assets.Scripts;
using UnityEngine;
using static Assets.Scripts.MyTileData;
using static Assets.Scripts.ObstacleData;

public class Obstacle : MonoBehaviour
{
    public Vector2Int position;
    public bool isAlive;
    public ObstacleData data;
    public ObstacleType obstacleType;
    public int currentHealth;

    public Sprite[] spriteVariants;

    private void Start()
    {
        isAlive = false;
    }

    public void Initialize(TileType tileType)
    {
        data = new ObstacleData(obstacleType, tileType);
        currentHealth = data.maxHealth;
        GetComponent<SpriteRenderer>().sprite = spriteVariants[(int)tileType];
    }

    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            isAlive = false;
            GetComponent<Renderer>().enabled = false;
        }
    }
}
