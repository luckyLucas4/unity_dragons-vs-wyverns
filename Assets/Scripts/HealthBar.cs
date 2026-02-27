using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Image sliderFill;
    public Color allyColor;
    public Color enemyColor;

    DragonUnit unit;
    
    public void Initialize(DragonUnit dragonUnit)
    {
        unit = dragonUnit;
    }

    private void Start()
    {
        Color color = unit.isAlly ? allyColor : enemyColor;
        sliderFill.color = color;
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
