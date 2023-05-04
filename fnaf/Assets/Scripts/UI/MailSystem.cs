using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class MailSystem : MonoBehaviour
{
    public GameObject paperProp;

    [SerializeField] GameObject mailUI;
    [SerializeField] Animator paperAnim;
    [SerializeField] AudioClip paperSound;
    [SerializeField] TextMeshProUGUI mailContent;

    DepthOfField depthOfField;
    GameManager gameManager;
    float blurFadeSpeed = 150;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);  // waits because witchout it, returns error
        GameManager.standardProfile.TryGet(out depthOfField);
        depthOfField.active = false;
        gameManager = FindObjectOfType<GameManager>();

        SetUpToNight();
        GameManager.OnConfigSet += SetUpToNight;
    }

    void SetUpToNight()
    {
        string path = Application.streamingAssetsPath + "/Mails" + "/Mail" + GameManager.actualNightIndex + ".txt";
        List<string> fileContent = File.ReadAllLines(path).ToList();
        StringBuilder sb = new StringBuilder();

        foreach (string line in fileContent)
        {
            sb.AppendLine(line);
        }
        mailContent.text = sb.ToString();
    }

    public IEnumerator OpenMail()
    {
        // when mail button click
        depthOfField.active = true;
        gameManager.ButtonsInSecurityRoomActiveChange(false);
        GameManager.soundsSource.Stop();
        GameManager.soundsSource.PlayOneShot(paperSound);
        GameManager.areSecurityRoomButtonsActive = false;

        // fade of blur
        depthOfField.focalLength.value = 0;
        while(depthOfField.focalLength.value < 50)
        {
            depthOfField.focalLength.value += Time.deltaTime * blurFadeSpeed;
            yield return null;
        }

        // when faded
        gameManager.MuffOrClearAmbient(true);
        mailUI.SetActive(true);
        yield return new WaitForSeconds(.1f);
        Time.timeScale = 0;
        paperProp.SetActive(false);  // it must be on end
    }

    public void HideMailHelper()
    {
        // using only to start corutine by button
        StartCoroutine(HideMail());
    }

    IEnumerator HideMail()
    {
        // hide mailUI
        Time.timeScale = 1;
        paperAnim.CrossFade("hidePaper", 0);
        gameManager.MuffOrClearAmbient(false);
        gameManager.ButtonsInSecurityRoomActiveChange(true);
        GameManager.soundsSource.PlayOneShot(paperSound);
        GameManager.areSecurityRoomButtonsActive = true;
        yield return new WaitForSeconds(.1f);
        mailUI.SetActive(false);

        while (depthOfField.focalLength.value > 0)
        {
            depthOfField.focalLength.value -= Time.deltaTime * blurFadeSpeed;
            yield return null;
        }
    }
}
