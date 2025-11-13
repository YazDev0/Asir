using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu_Functions : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    public ResultScreen_Functions resultScreenActive;
    //
    void Update()
    {
        if (resultScreenActive.resultScreenActive == false)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeSelf == false && resultScreenActive.resultScreenActive == false)
            {
                Pause();
                Debug.Log("PressedT");
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu.activeSelf == true &&
                     resultScreenActive.resultScreenActive == false)
            {
                Resume();
                Debug.Log("PressedF");
            } 
        }
        else pauseMenu.SetActive(false);
    }
    //
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }
    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1;
    }
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
}
