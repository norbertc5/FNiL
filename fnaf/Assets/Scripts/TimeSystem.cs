using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeSystem : MonoBehaviour
{
    [SerializeField] float secondsToMinute;  // how long time must pass to change seconds variable
    public float timeSpeed = 1;
    [SerializeField] TextMeshPro timeDisplayText;

    public static int hour = 12;
    int minutes;
    float seconds;

    private IEnumerator Start()
    {
        // it invokes on start because enemies can start walk on 12AM
        yield return new WaitForSeconds(1);
        GameManager.OnHourChanges?.Invoke();
    }

    void Update()
    {
        seconds += Time.deltaTime * timeSpeed;

        // when seconds variable is bigger than secondsToMinute, minutes variable increases
        if (seconds > secondsToMinute)
        {
            minutes++;
            UpdateText();
            seconds = 0;
        }

        // increase hour variable
        if(minutes >= 59)
        {
            hour++;

            if (hour > 12)
                hour = 1;

            UpdateText();

            if (hour >= 6)
            {
                GameManager g = FindObjectOfType<GameManager>();
                g.OnNightEnd?.Invoke();
            }

            GameManager.OnHourChanges?.Invoke();

            minutes = 0;
        }
    }

    void UpdateText()
    {
        // update the text on the clock
        timeDisplayText.text = hour.ToString() + " AM";
    }
}
