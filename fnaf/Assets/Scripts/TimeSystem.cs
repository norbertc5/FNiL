using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeSystem : MonoBehaviour
{
    public float secondsToMinute;
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
        seconds += Time.deltaTime;

        if(seconds > secondsToMinute)
        {
            minutes++;
            UpdateText();
            seconds = 0;
        }

        if(minutes >= 59)
        {
            hour++;

            if (hour > 12)
                hour = 1;

            UpdateText();

            if (hour == 6)
                GameManager.OnNightEnd?.Invoke();

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
