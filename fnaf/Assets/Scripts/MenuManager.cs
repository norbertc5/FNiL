using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject[] objectsWithAction;
    [SerializeField] GameObject lightObject;
    [SerializeField] TextMeshProUGUI playButtonText;
    int nightIndex = 1;

    void Start()
    {
        StartCoroutine(MakeEvent());
    }


    IEnumerator MakeEvent()
    {
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

    public void ExitButton()
    {
        Application.Quit();
    }

    public void ChangeNightToStart(int changeBy)
    {
        if((nightIndex > 1 && changeBy == -1) || (nightIndex < 6 && changeBy == 1))
        {
            nightIndex += changeBy;
            playButtonText.text = "Night " + nightIndex;
        }
    }
}
