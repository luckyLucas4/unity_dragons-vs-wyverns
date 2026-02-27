using Assets.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Assets.Scripts.DragonData;

public class InfoCard : MonoBehaviour
{

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI attackDamageText;
    public TextMeshProUGUI speedText;

    public GameObject specialInfoCard;
    public TextMeshProUGUI specialText;

    DragonUnit unit;
    bool showingSpecial;

    Dictionary<DragonType, string> specialInfo = new()
    {
        {DragonType.Plain, "Make a second attack. Has a cooldown of 2 turns after use." },
        {DragonType.Ice, "Move again after attacking. Has a cooldown of 1 turn."},
        {DragonType.Fire, "Make a breath attack after walking or attacking. " +
            "Has a cooldown of 4 turns after use."},
    };
    private void Start()
    {
        HideInfoCard();
    }
    public void ShowInfoCard(DragonUnit unit, bool showSpecial)
    {
        gameObject.SetActive(true);

        this.unit = unit;
        this.showingSpecial = showSpecial;

        nameText.text = unit.dragonName;
        healthText.text = $"{unit.currentHealth}/{unit.Data.MaxHealth}";
        attackDamageText.text = unit.Data.AttackDamage.ToString();

        // Normalize speed to passable tile cost
        int tileCost = (int)MyTileData.MoveCostType.Passable;
        speedText.text = $"{Mathf.Floor(unit.Data.Speed / tileCost)}";

        if (showSpecial)
        {
            specialText.text = specialInfo[unit.dragonType];
            specialInfoCard.SetActive(true);
        }
        else
            specialInfoCard.SetActive(false);
    }

    public void UpdateInfoCard()
    {
        if (IsShowing())
        {
            ShowInfoCard(unit, showingSpecial);
        }
    }

    public void HideInfoCard()
    {
        unit = null;
        specialInfoCard.SetActive(false);
        gameObject.SetActive(false);
    }

    public bool IsShowing() => unit != null;
}
