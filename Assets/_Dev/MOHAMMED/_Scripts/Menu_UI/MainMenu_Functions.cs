using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_Functions : MonoBehaviour
{
    public string startSceneName = "Level1";

    public void PlayScene()
    {
        SceneManager.LoadSceneAsync(startSceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
