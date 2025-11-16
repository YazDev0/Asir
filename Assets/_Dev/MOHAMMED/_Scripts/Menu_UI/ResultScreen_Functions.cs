using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultScreen_Functions : MonoBehaviour
{
    [SerializeField] GameObject resultScreen;
    // public Collider2D Trigger2D;
    public bool resultScreenActive;
    [SerializeField] private TextMeshProUGUI timeText;

    //
    void Update()
    {
        OnEnable();
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

    private void OnEnable()
    {
        float finalTime = LevelTimer.Instance.GetFinalTime();
        int minutes = Mathf.FloorToInt(finalTime / 60);
        int seconds = Mathf.FloorToInt(finalTime % 60);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
    //
    public void ShowResultScreen()
    {
        resultScreenActive = true;
        LevelTimer.Instance.StopTimer();
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
