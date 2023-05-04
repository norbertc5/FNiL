using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class LightCorridorButton : MonoBehaviour
{
    [SerializeField] Light corridorLight;
    Light buttonBacklight;
    AudioSource source;
    [HideInInspector] public bool isOn;
    AudioLowPassFilter lowPassFilter;

    private void Awake()
    {
        buttonBacklight = transform.Find("Backlight").GetComponent<Light>();
        source = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
    }

    private void Start()
    {
        // use to make this component able to change active state
    }

    private void OnMouseDown()
    {
        if (GameManager.areSecurityRoomButtonsActive)
        {
            if (!isOn)
            {
                corridorLight.gameObject.SetActive(true);
                buttonBacklight.color = Color.blue;
                StartCoroutine(LightRefraction());
                StartCoroutine(PlayLightSound());
                Battery.ChangePowerUsage(Battery.powerUsage + 1);
                isOn = true;
            }
            else
            {
                TurnOffLight();
            }
        }
        else
        {
            //play click sound when button is inactive(when battery run out)
            source.volume = 0.3f;
            source.PlayOneShot(GameManager.clickSound);
        }
    }

    IEnumerator LightRefraction()
    {
        // make light refractions, light turns off for short time and turns on again

        while(true)
        {
            yield return new WaitForSeconds(Random.Range(0.05f, 3f));
            corridorLight.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.02f);
            corridorLight.gameObject.SetActive(true);
        }
    }

    public IEnumerator PlayLightSound()
    {
        while(true)
        {
            source.Play();
            yield return new WaitForSecondsRealtime(source.clip.length);
        }
    }

    public IEnumerator LightCorridorBlink()
    {
        // turn off light on corridor, waits nad turn it on
        // using to hide that enemy apperad over door to security room
        corridorLight.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        corridorLight.gameObject.SetActive(isOn);
        yield return new WaitForSeconds(0.5f);
    }

    public void TurnOffLight()
    {
        corridorLight.gameObject.SetActive(false);
        buttonBacklight.color = new Color(0.6226415f, 0.6226415f, 0.6226415f);
        StopAllCoroutines();
        source.Stop();
        Battery.ChangePowerUsage(Battery.powerUsage - 1);
        isOn = false;
    }
}
