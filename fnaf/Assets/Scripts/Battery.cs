using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;

public class Battery : MonoBehaviour
{
    public static int powerUsage = 1;

    [SerializeField] GameObject[] powerUsageIconsHelper;
    [SerializeField] TextMeshPro batteryStateText;

    public static GameObject[] powerUsageIcons;
    public static int batteryState = 100;
    public float timeToDecreaseBattery = 10;
    float time = 10;

    private void Start()
    {
        powerUsageIcons = powerUsageIconsHelper;
        GameManager.OnConfigSet += SetUpBattery;
        ChangePowerUsage(1);
    }

    private void Update()
    {
        time -= Time.deltaTime * powerUsage;

        if(time <= 0)
        {
            batteryState--;
            batteryStateText.text = "Battery: " + batteryState + " %";
            time = timeToDecreaseBattery;
            GameManager.OnBatteryStateChange?.Invoke();

            if(batteryState <= 0)
            {
                GameManager.OnPowerRunOut?.Invoke();
                this.enabled = false;
            }
        }    
    }

    /// <summary>
    /// ChangeObject display of power usage on one of monitor in security room.
    /// </summary>
    /// <param name="newPowerUsage"></param>
    public static void ChangePowerUsage(int newPowerUsage)
    {
        if(newPowerUsage >= 1)
            powerUsage = newPowerUsage;

        // turn off all powerUsageIcons
        for (int i = 0; i < powerUsageIcons.Length; i++)
        {
            powerUsageIcons[i].SetActive(false);
        }

        int power = powerUsage;

        // power usage can't be too big
        if (powerUsage > 4)
            power = 4;

        // turn on right amoun of powerUsageIcons
        for (int i = 0; i < power; i++)
        {
            powerUsageIcons[i].SetActive(true);
        }
    }

    void SetUpBattery()
    {
        string path = Application.streamingAssetsPath + "/Configs" + "/TimeToDecreaseBattery" + ".txt";
        List<string> fileContent = File.ReadAllLines(path).ToList();
        timeToDecreaseBattery = float.Parse(fileContent[GameManager.actualNightIndex - 1]);
    }
}
