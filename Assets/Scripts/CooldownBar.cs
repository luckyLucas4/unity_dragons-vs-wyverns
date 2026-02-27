using UnityEngine;
using UnityEngine.UI;

public class CooldownBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxCooldown(int health)
    {
        slider.maxValue = health;
    }

    public void SetCooldown(int health)
    {
        slider.value = health;
    }
}
