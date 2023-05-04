using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // events
    public delegate void Action();
    public static Action OnBatteryStateChange;
    public static Action OnSecurityCamChanged;
    public static Action OnEnemyPositionChanged;
    public static Action OnHourChanges;
    public static Action OnJumpscare;
    public Action OnNightEnd;
    public static Action OnPowerRunOut;
    public static Action OnConfigSet;

    [Header("General")]
    [Space(6)]

    public static int actualNightIndex = 1;
    public static bool isLeftCorridorOccuped;
    public static bool isRightCorridorOccuped;
    public static bool areSecurityRoomButtonsActive = true;  // if buttons're inactive in gamplay (e.g. when no power)
    CamerasController camerasController;
    public static bool isGamePaused;
    bool isGameOver;

    [Space(10)]
    [Header("Helpers")]
    [Space(6)]

    [SerializeField] GameObject lightsObjectHelper;
    [SerializeField] EnemiesBehaviour[] enemiesHelper;

    public static GameObject lightsObject;
    public static EnemiesBehaviour[] enemies;

    [Space(10)]
    [Header("PostProcessing")]
    [Space(6)]

    [SerializeField] Volume postProcessingVolumeHelper;
    [SerializeField] VolumeProfile securityCamerasProfileHelper;
    [SerializeField] VolumeProfile standardProfileHelper;
    [SerializeField] VolumeProfile deathProfile;

    public static Volume postProcessingVolume;
    public static VolumeProfile securityCamerasProfile;
    public static VolumeProfile standardProfile;

    [Space(10)]
    [Header("Sounds")]
    [Space(6)]

    [SerializeField] AudioSource soundSourceHelper;
    public static AudioSource soundsSource;
    [SerializeField] AudioClip bellSound;
    [SerializeField] GameObject ambientSoundsHolder;
    [SerializeField] AudioClip deathSound;  // sound playing after jumpscare, when screen is covered by glitches
    [SerializeField] AudioClip powerDownSound;
    [SerializeField] AudioClip clickSoundHelper;
    [SerializeField] AudioSource jumpscareSource;
    [SerializeField] AudioMixerSnapshot defaultAmbient;
    [SerializeField] AudioMixerSnapshot muffedAmbient;
    public static AudioClip clickSound;

    [Space(10)]
    [Header("UI")]
    [Space(6)]

    [SerializeField] GameObject winUI;
    [SerializeField] GameObject mailButtonObject;
    [SerializeField] TextMeshProUGUI nightText;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject optionsTab;
    [SerializeField] GameObject buttonsHolder;

    [Space(10)]
    [Header("Thingh needed when\nbattery gone out")]
    [Space(6)]

    [SerializeField] GameObject[] turnOffWhenBatteyRanOut;
    [SerializeField] GameObject[] buttonsInSecurityRoom;
    [SerializeField] TextMeshPro batteryRanOutText;
    [SerializeField] GameObject enemyWhenNoPower;
    [SerializeField] GameObject secCamerasMonitor;  // using to turn it off when jumpscare

    public enum Rooms
    {
        LeftCorridor, RightCorridor, SupplyCloset, MainHallSouth, MainHallNorth, Kitchen, Toilets,
        Magazine, Isolations, CloakRoom, DontReveal
    };

    private void Start()
    {
        StartCoroutine(LateStart());
        actualNightIndex = PlayerPrefs.GetInt("nightIndex");
        TimeSystem.hour = 12;
        Battery.batteryState = 100;
        isGamePaused = false;

        #region Objects assignment

        postProcessingVolume = postProcessingVolumeHelper;
        securityCamerasProfile = securityCamerasProfileHelper;
        standardProfile = standardProfileHelper;
        soundsSource = soundSourceHelper;
        lightsObject = lightsObjectHelper;
        enemies = enemiesHelper;
        clickSound = clickSoundHelper;
        camerasController = FindObjectOfType<CamerasController>();

        #endregion

        #region Events subscription

        OnJumpscare += GameOver;
        OnNightEnd += Win;
        OnPowerRunOut += PowerRunOut;

        #endregion

        #region Enables encyclopedia entries

        if (actualNightIndex == 1)
        {
            PlayerPrefs.SetString("Der Kiefergezahnt", "true");
            PlayerPrefs.SetString("Skin-Man", "true");
            PlayerPrefs.SetString("You", "true");
            PlayerPrefs.SetString("The Boss", "true");
            PlayerPrefs.SetString("The Laboratory", "true");
            PlayerPrefs.SetString("IsSthNewInEncyclopedia", "true");
            PlayerPrefs.Save();
        }
        else if(actualNightIndex == 2)
        {
            PlayerPrefs.SetString("Ghoul", "true");
            PlayerPrefs.SetString("Clawler", "true");
            PlayerPrefs.SetString("IsSthNewInEncyclopedia", "true");
            PlayerPrefs.Save();
        }

        #endregion

        #region UI set

        nightText.text = "Night " + actualNightIndex;

        #endregion
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(.1f);
        OnConfigSet?.Invoke();  // it's ivoking later to other classes could subscribe event
    }

    private void Update()
    {
        // when esc pressed, time stops and pause screen appears, pause can't be enable when game over
        if(Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            Pause();
        }
    }

    void GameOver()
    {
        ambientSoundsHolder.SetActive(false);
        GetComponent<TimeSystem>().enabled = false;
        StartCoroutine(ChangePPtoDeathProfile());
        camerasController.CloseSecurityCameras();
        secCamerasMonitor.SetActive(false);
        camerasController.gameObject.SetActive(false);
        StartCoroutine(SwitchSceneToMenu());
        ButtonsInSecurityRoomActiveChange(false);
        PlayerPrefs.SetInt("NextNightIndex", actualNightIndex);

        // turn off all enemies
        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    IEnumerator SwitchSceneToMenu()
    {
        yield return new WaitForSeconds(7);
        SceneManager.LoadScene(0);
    }

    void Win()
    {
        this.gameObject.GetComponent<TimeSystem>().enabled = false;
        ambientSoundsHolder.SetActive(false);
        camerasController.CloseSecurityCameras();
        secCamerasMonitor.SetActive(false);
        camerasController.gameObject.SetActive(false);
        enemyWhenNoPower.SetActive(false);
        ButtonsInSecurityRoomActiveChange(false);

        StartCoroutine(PlayBellSound());
        StartCoroutine(WinUIFade());

        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }

        // when player wins 5th, special things unlock
        if (actualNightIndex == 5/* && PlayerPrefs.GetString("HasRevaledForNight" + actualNightIndex) != "true"*/)
        {
            PlayerPrefs.SetString("HasFinished5thNight", "true");
            PlayerPrefs.SetString("Console", "true");
            PlayerPrefs.SetString("IsSthNewInEncyclopedia", "true");
            PlayerPrefs.Save();
        }

        #region Set UI

        buttonsHolder.SetActive(true);
        buttonsHolder.transform.Find("Resume").gameObject.SetActive(false);

        // turning on or off next night button
        if (actualNightIndex == 6)
            buttonsHolder.transform.Find("NextNight").gameObject.SetActive(false);
        else
            buttonsHolder.transform.Find("NextNight").gameObject.SetActive(true);

        // when there is something new in encyclopedia, text on win screen will appear
        if ((actualNightIndex != 5 && PlayerPrefs.GetString("IsSthNewInEncyclopedia") == "true" && 
           PlayerPrefs.GetString("HasRevaledForNight" + actualNightIndex) != "true") 
           || (actualNightIndex == 5 && PlayerPrefs.GetString("HasRevaledCongratulations") != "true"))
            winUI.transform.Find("NewEntryText").gameObject.SetActive(true);
        else
            winUI.transform.Find("NewEntryText").gameObject.SetActive(false);

        #endregion

        // when player click play button in menu, correct night will start
        if(actualNightIndex != 6)
            PlayerPrefs.SetInt("NextNightIndex", actualNightIndex += 1);
        else
            PlayerPrefs.SetInt("NextNightIndex", actualNightIndex);

        PlayerPrefs.Save();
    }

    void PowerRunOut()
    {
        camerasController.CloseSecurityCameras();
        lightsObject.SetActive(false);
        ambientSoundsHolder.SetActive(false);
        soundsSource.PlayOneShot(powerDownSound);
        areSecurityRoomButtonsActive = false;
        enemyWhenNoPower.SetActive(true);

        StartCoroutine(BatteryRanOutTextBlink());

        // turn off all objects like text on monitors on desk
        foreach(GameObject gameObject in turnOffWhenBatteyRanOut)
        {
            gameObject.SetActive(false);
        }

        // turn off doors in security room and lights in corridors
        foreach (GameObject button in buttonsInSecurityRoom)
        {
            if (button.GetComponent<DoorButton>())
            {
                DoorButton doorScript = button.GetComponent<DoorButton>();
                if (doorScript.isOn)
                    doorScript.UseDoor();
            }
            else if (button.GetComponent<LightCorridorButton>())
            {
                LightCorridorButton lightScript = button.GetComponent<LightCorridorButton>();
                if(lightScript.isOn)
                    lightScript.TurnOffLight();
            }
        }

        // turn off powerUsageIcons
        foreach (GameObject icon in Battery.powerUsageIcons)
        {
            icon.SetActive(false);
        }

        foreach (EnemiesBehaviour enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayBellSound()
    {
        // play bell sound 6 times (it means that that there is 6AM)
        soundsSource.Stop();

        for (int i = 0; i < 6; i++)
        {
            yield return null;
            soundsSource.PlayOneShot(bellSound);
            yield return new WaitForSeconds(bellSound.length + 0.5f);
        }
    }

    IEnumerator WinUIFade()
    {
        // win screen show
        CanvasGroup canvasGroup = winUI.GetComponent<CanvasGroup>();

        while(canvasGroup.alpha < 1f)
        {
            winUI.GetComponent<CanvasGroup>().alpha += 0.1f * 10 * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ChangePPtoDeathProfile()
    {
        // change post processing profile to death
        yield return new WaitForSeconds(1);
        postProcessingVolume.profile = deathProfile;
        soundsSource.PlayOneShot(deathSound);
    }

    IEnumerator BatteryRanOutTextBlink()
    {
        yield return new WaitForSeconds(2);
        for (int i = 0; i <= 5; i++)
        {
            batteryRanOutText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            batteryRanOutText.gameObject.SetActive(false);
            yield return new WaitForSeconds(1);
        }
    }

    public void Jumpscare(GameObject toTurnOff, GameObject toTurnOn, AudioClip jumpscareSound)
    {
        // make jumpscare with toTurnOn
        // avoid error when cameraController is inactive
        if (camerasController.gameObject.activeSelf)
            camerasController.CloseSecurityCameras();

        PlayerCameraMovement playerCam = FindObjectOfType<PlayerCameraMovement>();

        toTurnOff.SetActive(false);
        toTurnOn.SetActive(true);
        playerCam.GetComponentInParent<Animator>().enabled = true;
        playerCam.enabled = false;
        GetComponent<MailSystem>().enabled = false;  // make that mail button can hide when jumpscare
        mailButtonObject.SetActive(false);
        jumpscareSource.PlayOneShot(jumpscareSound);
        isGameOver = true;
        OnJumpscare?.Invoke();
    }

    public static IEnumerator MianLightBlinking()
    {
        // blinking of light in security room
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.02f, 0.07f));
            lightsObject.SetActive(false);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.02f, 0.07f));
            lightsObject.SetActive(true);
        }

        // sometimes it can be turned off when stop blinking
        lightsObject.SetActive(true);
    }

    /// <summary>
    /// Set ambient sounds to muffed or clear it. If muffOrClear = true ambient muffs, else clears.
    /// </summary>
    /// <param name="muffOrClear"></param>
    public void MuffOrClearAmbient(bool muffOrClear)
    {
        LightCorridorButton[] lightButtons = FindObjectsOfType<LightCorridorButton>();

        // muff or clear ambient sounds
        if (muffOrClear)
            muffedAmbient.TransitionTo(.1f);
        else
            defaultAmbient.TransitionTo(.1f);

        // also change lightButtons sound
        foreach(LightCorridorButton lightButton in lightButtons)
        {
            lightButton.GetComponent<AudioLowPassFilter>().enabled = muffOrClear;
        }
    }

    /// <summary>
    /// When newActiveState is true sounds will turn on and vice versa.
    /// </summary>
    /// <param name="newActiveState"></param>
    public void ButtonsInSecurityRoomActiveChange(bool newActiveState)
    {
        // set enable of colliders in all of LightCorridorButton and DoorButton to newActiveState
        // so when collider is unenabled, player can't use button
        LightCorridorButton[] lightButtons = FindObjectsOfType<LightCorridorButton>();
        DoorButton[] doorButtons = FindObjectsOfType<DoorButton>();

        foreach(LightCorridorButton lightButton in lightButtons)
        {
            lightButton.GetComponent<MeshCollider>().enabled = newActiveState;
        }

        foreach (DoorButton doorButton in doorButtons)
        {
            doorButton.GetComponent<MeshCollider>().enabled = newActiveState;
        }
    }

    public void OpenOrCloseOptions()
    {
        Options options = FindObjectOfType<Options>();
        optionsTab.SetActive(!optionsTab.activeSelf);
    }

    public void LoadScene(int index)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(index);
    }

    public void NextNight()
    {
        PlayerPrefs.SetInt("nightIndex", PlayerPrefs.GetInt("nightIndex") + 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }

    public void Resume()
    {
        Pause();
    }

    void Pause()
    {
        isGamePaused = !isGamePaused;
        pauseScreen.SetActive(isGamePaused);
        ambientSoundsHolder.SetActive(!isGamePaused);
        ButtonsInSecurityRoomActiveChange(!isGamePaused);
        buttonsHolder.SetActive(isGamePaused);
        buttonsHolder.transform.Find("NextNight").gameObject.SetActive(false);

        if(optionsTab.activeSelf)
            optionsTab.SetActive(isGamePaused);

        #region Audio mute

        if (isGamePaused)
            AudioListener.volume = 0;
        else
            AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1);

        #endregion

        // stop or resume time
        if (isGamePaused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }
}
