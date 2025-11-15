using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance; // So other scripts can access it
    private float startTime;
    private float finishTime;
    private bool isTiming = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        startTime = Time.time;
        isTiming = true;
    }

    public void StopTimer()
    {
        if (isTiming)
        {
            finishTime = Time.time - startTime;
            isTiming = false;
        }
    }

    public float GetFinalTime()
    {
        return finishTime;
    }

    public float GetCurrentTime()
    {
        return isTiming ? Time.time - startTime : finishTime;
    }
}