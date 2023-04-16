using System.Collections;
using System.Collections.Generic;
using System.Data;
using SmartConsole;
using UnityEngine;

public class Console : CommandBehaviour
{
    GameManager gameManager;
    MailSystem mailSystem;

    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
        mailSystem = GetComponent<MailSystem>();
    }

    [Command]
    void MoveEnemyToDoor(int enemyIndex)
    {
        Debug.Log("In next step enemy be in front of security room door.");
        StartCoroutine(GameManager.enemies[enemyIndex].ChangePosition());
        GameManager.enemies[enemyIndex].nextPosition = GameManager.enemies[enemyIndex].allPossiblePositions.Length - 2;
    }

    [Command]
    void ResetEnemyPosition(int enemyIndex)
    {
        Debug.Log("In next step enemy'll be at he's first position.");
        GameManager.enemies[enemyIndex].nextPosition = 0;
    }

    [Command]
    void SetHourTo(int hour)
    {
        Debug.Log("Hour was changed.");
        TimeSystem.hour = hour;
        GameManager.OnHourChanges?.Invoke();
    }

    [Command]
    void SetBatteryStateTo(int newBatteryState)
    {
        Debug.Log("Battery state was changed.");
        Battery.batteryState = newBatteryState;
    }

    [Command]
    void SetTimeSpeed(float newSpeed)
    {
        Debug.Log("Time speed was set to " + newSpeed + " (default is 1).");
        TimeSystem timeSystem = FindObjectOfType<TimeSystem>();
        timeSystem.timeSpeed = newSpeed;
    }

    [Command]
    void StartCrawler()
    {
        Debug.Log("Crawler will come in a moment.");
        VentilationEnemy ventEnemy = FindObjectOfType<VentilationEnemy>();
        ventEnemy.canCountTimeToChangeState = true;
    }

    [Command]
    void SetNightIndex(int night)
    {
        Debug.Log("Night index is set to " + night);
        GameManager.actualNightIndex = night;
        GameManager.OnConfigSet?.Invoke();
    }

    [Command]
    void ShowMailProp()
    {
        Debug.Log("Mail prop appeard.");
        mailSystem.paperProp.SetActive(true);
    }

    [Command]
    void Help()
    {
        Debug.Log("\n<b>Possible commands:</b> \n" +

            "\n <b> For enemies:</b>" +
            "\n  -MoveEnemyToDoor\n  -ResetEnemyPosition\n  -StartCrawler\n" +

            "\n <b> For time:</b>" +
            "\n  -SetHourTo\n  -SetTimeSpeed\n" +

            "\n <b> For battery:</b>" +
            "\n  -SetBatteryStateTo" +

            "\n\n<b>To autocomplete press TAB.</b>");
    }
}
