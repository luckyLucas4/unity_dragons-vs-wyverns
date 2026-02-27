using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnLabel : MonoBehaviour
{
    public TextMeshProUGUI allyTurnText;
    public TextMeshProUGUI enemyTurnText;

    public void UpdateTurnLabel(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            enemyTurnText.transform.gameObject.SetActive(false);
            allyTurnText.transform.gameObject.SetActive(true);
        }
        else
        {
            enemyTurnText.transform.gameObject.SetActive(true);
            allyTurnText.transform.gameObject.SetActive(false);
        }
    }
}
