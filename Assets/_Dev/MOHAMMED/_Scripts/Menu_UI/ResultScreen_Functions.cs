using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen_Functions : MonoBehaviour
{
    [SerializeField] GameObject resultScreen;
    public bool resultScreenActive;
    //
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && resultScreen.activeSelf == false)
        {
            resultScreenActive = true;
            ShowResultScreen();
            Debug.Log("PressedT");
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && resultScreen.activeSelf == true)
        {
            ShowResultScreen();
            Debug.Log("PressedF");
        }
    }
    //
    public void ShowResultScreen()
    {
        resultScreen.SetActive(true);
        Time.timeScale = 0;
    }
    public void Home() // Main Menu
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1;
    }
    public void Continue()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1;
    }
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
}
