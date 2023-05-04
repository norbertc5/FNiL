using System.Collections;
using UnityEngine;
using URPGlitch.Runtime.DigitalGlitch;
using TMPro;
using UnityEngine.Audio;

public class CamerasController : MonoBehaviour
{
    [Header("Main")]
    [Space(6)]

    [SerializeField] Animator camerasMonitorAnimObject;
    [SerializeField] Camera playerCamera;
    //[SerializeField] EnemiesBehaviour enemy;

    [Space(10)]
    [Header("UI")]
    [Space(6)]

    [SerializeField] GameObject securityCamerasUI;
    [SerializeField] TextMeshProUGUI batteryUIText;
    [SerializeField] GameObject batteryUsageUI;
    [SerializeField] RectTransform playerMapSign;

    [SerializeField] Transform securityCamerasParent;
    public static GameObject[] securityCameras = new GameObject[10];
    [SerializeField] GameObject[] securityCamsButtons;
    [SerializeField] GameObject consoleUI;

    [Space(10)]
    [Header("Sound")]
    [Space(6)]

    [SerializeField] AudioClip camerasOpenSound;
    [SerializeField] AudioClip camerasGlitchSound;
    [SerializeField] AudioClip changeSecurityCamSound;
    AudioSource source;

    public static bool areSecurityCamerasOpen;
    float timeToOpenSecurityCams = 0.5f;  // it's needed to avoid error
    DigitalGlitchVolume digitalGlitchIntensity;
    public static int actualSecurityCam;
    public static bool areSecurityCamsGlitched;

    private void Start()
    {
        GameManager.OnBatteryStateChange += UpdateUIBatteryInfo;
        GameManager.securityCamerasProfile.TryGet(out digitalGlitchIntensity);
        source = GetComponent<AudioSource>();

        for (int i = 0; i < securityCamerasParent.childCount; i++)
        {
            securityCameras[i] = securityCamerasParent.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        timeToOpenSecurityCams -= Time.deltaTime;

        // open cameras by press space and console is closed and time scale isn't 0
        if (Input.GetKeyDown(KeyCode.Space) && timeToOpenSecurityCams <= 0 && !consoleUI.activeSelf && Time.timeScale != 0)
        {
            if (areSecurityCamerasOpen)
                CloseSecurityCameras();
            else
                OpenSecurityCameras();

            source.PlayOneShot(camerasOpenSound);
            timeToOpenSecurityCams = 1f;
        }
    }

    void OpenSecurityCameras()
    {
        camerasMonitorAnimObject.gameObject.SetActive(true);
        camerasMonitorAnimObject.SetBool("isOpening", true);
        UpdateUIBatteryInfo();
        Battery.ChangePowerUsage(Battery.powerUsage + 1);
        UpdateBatteryUsageUI();
        StartCoroutine(ToggleSecurityCamsView(true));
        StartCoroutine(PlayerMapSignAnim());
        StartCoroutine(ModifyGlitchIntensity(true));
        FindObjectOfType<GameManager>().MuffOrClearAmbient(true);
    }

    private void OnMouseDown()
    {
        // this script is attached to cameras button on one of monitor on desk in security room,
        // OnMouse methods works on it
        if(GameManager.areSecurityRoomButtonsActive)
        {
            source.PlayOneShot(camerasOpenSound);
            OpenSecurityCameras();
        }
    }

    private void OnMouseEnter()
    {
        // make cameras button bigger when it touch cursor
        if (GameManager.areSecurityRoomButtonsActive)
            transform.localScale = new Vector3(0.22f, 0.22f, 0.2f);
    }

    private void OnMouseExit()
    {
        // make cameras button smaller when it touch cursor
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }

    /// <summary>
    /// Make delay the camerasMonitorAnimObject so it can finish the animation.
    /// </summary>
    /// <param name="isOn"></param>
    IEnumerator ToggleSecurityCamsView(bool isOn)
    {
        yield return new WaitForSeconds(0.2f);
        if (isOn)
            SwitchCamera(actualSecurityCam);
        else
            camerasMonitorAnimObject.gameObject.SetActive(false);
    }


    /// <summary>
    /// ChangeObject actual working camera.
    /// </summary>
    /// <param name="securityCameraID"></param>
    /// <param name="backToMain"></param>
    void SwitchCamera(int securityCameraID, bool backToMain = false)
    {
        // when backToMain equals to true, working camera backs to main

        playerCamera.gameObject.SetActive(backToMain);
        securityCamerasUI.SetActive(!backToMain);
        actualSecurityCam = securityCameraID;

        for (int i = 0; i < securityCameras.Length; i++)
        {
            securityCameras[i].SetActive(false);
        }

        securityCameras[securityCameraID].SetActive(!backToMain);

        if (backToMain)
        {
            areSecurityCamerasOpen = false;
            GameManager.postProcessingVolume.profile = GameManager.standardProfile;
            camerasMonitorAnimObject.SetBool("isOpening", false);

            if(this.gameObject.activeSelf)
                StartCoroutine(ToggleSecurityCamsView(false));
        }
        else
        {
            areSecurityCamerasOpen = true;
            //RenderSettings.ambientLight = new Color(0.04f, 0.04f, 0.04f);
            GameManager.postProcessingVolume.profile = GameManager.securityCamerasProfile;
            GameManager.OnSecurityCamChanged?.Invoke();
        }
    }


    public void CloseSecurityCameras()
    {
        // it execute when player clicks hide button when security cameras are open
        SwitchCamera(actualSecurityCam, true);
        Battery.ChangePowerUsage(Battery.powerUsage - 1);
        FindObjectOfType<GameManager>().MuffOrClearAmbient(false);

       // if(areSecurityCamerasOpen)
            source.PlayOneShot(camerasOpenSound);
    }

    /// <summary>
    /// Update battery state in UI.
    /// </summary>
    void UpdateUIBatteryInfo()
    {
        batteryUIText.text = "Battery: " + Battery.batteryState.ToString() + " %";
    }


    /// <summary>
    /// Update battery usage indicator in UI.
    /// </summary>
    public void UpdateBatteryUsageUI()
    {
        // it works as same as battery usage indicator in Battery.cs script

        for (int i = 0; i < batteryUsageUI.transform.childCount; i++)
        {
            batteryUsageUI.transform.GetChild(i).gameObject.SetActive(false);
        }

        int usage = 0;

        if (Battery.powerUsage > 4)
            usage = 4;
        else
            usage = Battery.powerUsage;

        for (int i = 0; i < usage; i++)
        {
            batteryUsageUI.transform.GetChild(i).gameObject.SetActive(true);
        }
    }


    IEnumerator PlayerMapSignAnim()
    {
        // animation of circle in map in cameras screen

        while (true)
        {
            yield return new WaitForSeconds(1);
            playerMapSign.localScale = new Vector3(1.2f, 1.2f, 1);
            yield return new WaitForSeconds(1);
            playerMapSign.localScale = new Vector3(1, 1, 1);
        }
    }


    public void ChangeSecurityCameraHelper(int index)
    {
        // using to invoke ChangeSecurityCamera corutine when security cam button clicked
        // Corutines can'timeToChangeState be invoke in OnClick
        if (actualSecurityCam != index)
        {
            StartCoroutine(ModifyGlitchIntensity());
            StartCoroutine(ChangeSecurityCamera(index));
        }
    }


    public IEnumerator ChangeSecurityCamera(int cameraIndex)
    {
        // open new security cam after 1 second to glitch can end
        if (actualSecurityCam != cameraIndex)
        {
            GameManager.soundsSource.PlayOneShot(changeSecurityCamSound);
            yield return new WaitForSeconds(0.4f);

            if(areSecurityCamerasOpen)
                SwitchCamera(cameraIndex);
        }       
    }


    /// <summary>
    /// Make glitch on security cameras stronger for animationToPlay moment and, after delay, restores intensity.
    /// </summary>
    /// <param name="skipFirstPart"></param>
    IEnumerator ModifyGlitchIntensity(bool skipFirstPart = false)
    {
        // skipFirstPart allows to skip part of glitch, when it become stronger, when it's true glitch is strong from start

        float delay = 0.13f;
        float maxIntensity = 0.9f;

        if(!skipFirstPart)
        {
            digitalGlitchIntensity.intensity.value = 0.009f;
            yield return new WaitForSeconds(delay);
            digitalGlitchIntensity.intensity.value = 0.3f;
            yield return new WaitForSeconds(delay);
        }

        if (skipFirstPart)
            delay = 0.7f;

        digitalGlitchIntensity.intensity.value = maxIntensity;
        yield return new WaitForSeconds(delay);
        digitalGlitchIntensity.intensity.value = maxIntensity / 2;
        yield return new WaitForSeconds(delay);
        digitalGlitchIntensity.intensity.value = 0.009f;
    }

    public IEnumerator CameraGlitched()
    {
        // turn off all security cameras light, play glitch sound and turn on light
        // it makes that player doesn't see anything when enemies moving

        areSecurityCamsGlitched = true;
        Coroutine coroutine = null;

        foreach (GameObject item in securityCameras)
        {
            item.transform.GetChild(0).gameObject.SetActive(false);
        }

        // glitch sound is playing only if security cameras are open
        if(areSecurityCamerasOpen)
        {
            GameManager.soundsSource.PlayOneShot(camerasGlitchSound);
        }
        coroutine = StartCoroutine(StopGlitchSoundIfSecurityCamsClosed());

        yield return new WaitForSeconds(Random.Range(0.3f, 1.4f));

        foreach (GameObject item in securityCameras)
        {
            item.transform.GetChild(0).gameObject.SetActive(true);
        }
        StopCoroutine(coroutine);        
        areSecurityCamsGlitched = false;
    }

    IEnumerator StopGlitchSoundIfSecurityCamsClosed()
    {
        // if player turns off security cams durring glitch sound it'll stop this sound
        while (true)
        {
            if (!areSecurityCamerasOpen)
                GameManager.soundsSource.Stop();

            yield return new WaitForEndOfFrame();
        }
    }
}