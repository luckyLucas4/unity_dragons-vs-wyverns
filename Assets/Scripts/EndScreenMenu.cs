using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenMenu : MonoBehaviour
{
    public GameObject victoryMenu;
    public GameObject defeatMenu;
    public void LoadNextLevel()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    public void RestartLevel()
        => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void ShowMenu(bool playerWin)
    {
        if (playerWin)
            victoryMenu.SetActive(true);
        else
            defeatMenu.SetActive(true);
    }
}
