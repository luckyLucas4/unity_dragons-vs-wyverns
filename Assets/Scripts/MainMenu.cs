using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public void PlayGame()
    {
        SceneManager.LoadScene(GetLevelNumber());
    }

    public void LeftArrowPress()
    {
        int nextNumber = GetLevelNumber() - 1;
        if (nextNumber > 0)
        {
            numberText.text = nextNumber.ToString();
        }
    }

    public void RightArrowPress()
    {
        int nextNumber = GetLevelNumber() + 1;
        if (nextNumber < SceneManager.sceneCountInBuildSettings)
        {
            numberText.text = nextNumber.ToString();
        }
    }

    int GetLevelNumber() => int.Parse(numberText.text);
}
