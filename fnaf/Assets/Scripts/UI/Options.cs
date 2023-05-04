using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Options : MonoBehaviour
{
    [Header("General")]
    [Space(6)]

    [SerializeField] Slider volumeSlider;
    [SerializeField] Toggle fullScreenToggle;
    [SerializeField] Toggle fpsCounterToggle;
    [SerializeField] TMP_Dropdown resolutionDropdown;

    [Space(10)]
    [Header("Other")]
    [Space(6)]

    [SerializeField] TextMeshProUGUI fpsCounterText;
    Resolution[] resolutions;

    void Start()
    {
        #region Load settigs and set UI

        // volume
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1);
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1);

        // full screen
        if (PlayerPrefs.GetString("FullScreen", "True") == "True")
        {
            Screen.fullScreen = true;
            fullScreenToggle.isOn = true;
        }
        else if (PlayerPrefs.GetString("FullScreen", "True") == "False")
        {
            Screen.fullScreen = false;
            fullScreenToggle.isOn = false;
        }

        // fps counter
        if(PlayerPrefs.GetString("FPSCounter", "False") == "True")
        {
            fpsCounterText.gameObject.SetActive(true);
            fpsCounterToggle.isOn = true;
        }
        else if(PlayerPrefs.GetString("FPSCounter", "False") == "False")
        {
            fpsCounterText.gameObject.SetActive(false);
            fpsCounterToggle.isOn = false;
        }

        // resolution
        resolutionDropdown.ClearOptions();
        resolutions = Screen.resolutions;
        List<string> resolutionsList = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            resolutionsList.Add(option);
        }

        resolutionDropdown.AddOptions(resolutionsList);
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 15);

        Screen.SetResolution(resolutions[PlayerPrefs.GetInt("ResolutionIndex", 15)].width, 
            resolutions[PlayerPrefs.GetInt("ResolutionIndex", 15)].height, Screen.fullScreen);

        #endregion

        // option must be turned on when game starts, turns off options after set everything
        this.gameObject.SetActive(false);                                          
    }

    public void Volume()
    {
        AudioListener.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();
    }

    public void FullScreen()
    {
        Screen.fullScreen = fullScreenToggle.isOn;
        PlayerPrefs.SetString("FullScreen", fullScreenToggle.isOn.ToString());
        PlayerPrefs.Save();
    }

    public void FPSCounter()
    {
        fpsCounterText.gameObject.SetActive(fpsCounterToggle.isOn);
        PlayerPrefs.SetString("FPSCounter", fpsCounterToggle.isOn.ToString());
        PlayerPrefs.Save();
    }

    public void Resolution()
    {
        Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.Save();
    }
}
