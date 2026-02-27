using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterface : MonoBehaviour
{
    [SerializeField] InfoCard infoCard;
    [SerializeField] TurnLabel turnLabel;
    [SerializeField] EndScreenMenu endScreenMenu;

    public bool InfoCardShowing { get => infoCard.IsShowing(); }

    public void ShowEndScreen(bool playerWin) 
        => endScreenMenu.ShowMenu(playerWin);

    public void ShowInfoCard(DragonUnit unit, bool showSpecial)
        => infoCard.ShowInfoCard(unit, showSpecial);

    public void HideInfoCard() => infoCard.HideInfoCard();

    public void UpdateInfoCard() => infoCard.UpdateInfoCard();

    public void UpdateTurnLabel(bool isPlayerTurn) 
        => turnLabel.UpdateTurnLabel(isPlayerTurn);
}
