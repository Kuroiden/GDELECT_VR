using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 300f; // 5 minutes = 300 seconds
    public TextMeshProUGUI timerText;

    private bool timerRunning = true;

    void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;
                timerText.text = "Time's up!";
                Debug.Log("Timer finished");
            }
        }
    }

    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}