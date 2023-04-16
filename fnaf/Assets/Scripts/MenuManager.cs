using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    [Header("Random events")]
    [Space(6)]

    [SerializeField] GameObject[] objectsWithAction;
    [SerializeField] GameObject lightObject;


    [Space(10)]
    [Header("Encyclopedia")]
    [Space(6)]

    [SerializeField] GameObject encyclopediaObject;
    public TextMeshProUGUI textInEncyclopedia;
    public Image imageInEncyclopedia;
    public float encyclopediaTextYPos;
    public const float DEFAULT_TEXT_SIZE_IN_ENCYCLOPEDIA_TAB_BUTTON = 40;
    [SerializeField] GameObject exclamationMarkObject;

    [SerializeField] TextMeshProUGUI playButtonText;
    int nightIndex = 1;

    void Start()
    {
        StartCoroutine(MakeEvent());
        encyclopediaTextYPos = textInEncyclopedia.rectTransform.position.y;

        Debug.Log(PlayerPrefs.GetString("HasRevaledForNight" + nightIndex) != "true");

        // exclamation mark appears only if player starts night once. If player starts same night 2nd time, exclamation mark won't appear
        if((PlayerPrefs.GetString("IsSthNewInEncyclopedia") == "true" || PlayerPrefs.GetString("HasFinished5thNight") == "true") 
            && PlayerPrefs.GetString("HasRevaledForNight" + nightIndex) != "true")
        {
            exclamationMarkObject.SetActive(true);
            PlayerPrefs.SetString("HasRevaledForNight" + nightIndex, "true");
        }
    }


    IEnumerator MakeEvent()
    {
        // wait some time and show change on scene
        while (true) 
        {
            yield return new WaitForSeconds(Random.Range(10, 31));

            int r = Random.Range(0, 3);

            switch (r)
            {
                case 0: StartCoroutine(ShowEnemy(objectsWithAction[r])); break;
                case 1: StartCoroutine(ShowEnemy(objectsWithAction[r])); break;
                case 2: StartCoroutine(LightBlink()); break;
            }
        }
    }

    IEnumerator ShowEnemy(GameObject enemy)
    {
        // show enemy, wait short time and hide enemy
        yield return new WaitForSeconds(1);
        StartCoroutine(LightBlink());
        enemy.SetActive(true);
        yield return new WaitForSeconds(Random.Range(3, 7));
        StartCoroutine(LightBlink());
        enemy.SetActive(false);
    }
    IEnumerator LightBlink()
    {
        // blinking of light in security room
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(Random.Range(0.02f, 0.07f));
            lightObject.SetActive(false);
            yield return new WaitForSeconds(Random.Range(0.02f, 0.07f));
            lightObject.SetActive(true);
        }
    }

    public void PlayButton()
    {
        PlayerPrefs.SetInt("nightIndex", nightIndex);
        SceneManager.LoadScene(1);
    }

    public void EncyclopediaButon()
    {
        encyclopediaObject.SetActive(true);
        PlayerPrefs.SetString("IsSthNewInEncyclopedia", "false");
        exclamationMarkObject.SetActive(false);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void ChangeNightToPlay(int changeBy)
    {
        // this function is using by buttons to change night to play
        // night to play mustn't be less than 1 and bigger than 6
        if((nightIndex > 1 && changeBy == -1) || (nightIndex < 6 && changeBy == 1))
        {
            nightIndex += changeBy;
            playButtonText.text = "Night " + nightIndex;
        }
    }

    public void CloseAllTabs()
    {
        // close all tabs in menu
        encyclopediaObject.SetActive(false);
    }
}
