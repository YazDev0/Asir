using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen_Functions : MonoBehaviour
{
    [SerializeField] GameObject resultScreen;
    // public Collider2D Trigger2D;
    public bool resultScreenActive;
    //
    void Update()
    {
        
    }
    //

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ShowResultScreen();
            Debug.Log("is triggered");
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
