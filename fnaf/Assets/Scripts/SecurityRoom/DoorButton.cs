using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorButton : MonoBehaviour
{
    [SerializeField] Animator doorAnimObject;
    [SerializeField] AudioClip doorUsingSound;
    [SerializeField] AudioClip beepSound;

    Light buttonBacklight;
    AudioSource source;
    [HideInInspector] public bool isOn;
    bool canUse = true;
    Coroutine coroutine;  // reference to ButtonBacklightBlink corutine. Using to stop this.
    bool isUsingByEnemy;

    private void Awake()
    {
        buttonBacklight = transform.Find("Backlight").GetComponent<Light>();
        source = GetComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        if(GameManager.areSecurityRoomButtonsActive)
        {
            if (canUse)
            {
                if (isUsingByEnemy)
                    UseDoor(1, true);
                else
                    UseDoor();
            }
        }
        else
        {
            // play click sound when button is inactive (when battery run out)
            source.pitch = 1;
            source.PlayOneShot(GameManager.clickSound);
        }
    }

    IEnumerator DoorUseDelay()
    {
        // make that player can'timeToChangeState close door while it opening
        yield return new WaitForSeconds(doorAnimObject.GetCurrentAnimatorStateInfo(0).length);
        canUse = true;
    }

    /// <summary>
    /// Changes state of security door.
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="isUsingByEnemy"></param>
    /// <param name="canPlaySound"></param>
    public void UseDoor(float speed = 1, bool isUsingByEnemy = false, bool canPlaySound = true)
    {
        doorAnimObject.SetFloat("speed", speed);

        if (!isOn)
        {
            // close
            buttonBacklight.color = new Color(0.3124003f, 0.7830189f, 0.2031417f);
            doorAnimObject.SetBool("isClosing", true);
            Battery.ChangePowerUsage(Battery.powerUsage + 1);
            isOn = true;

            if(isUsingByEnemy)
                StopCoroutine(coroutine);
        }
        else
        {
            // open
            buttonBacklight.color = new Color(0.6226415f, 0.6226415f, 0.6226415f);
            doorAnimObject.SetBool("isClosing", false);
            doorAnimObject.SetBool("isUsingByEnemy", false);
            Battery.ChangePowerUsage(Battery.powerUsage - 1);
            isOn = false;
        }

        if(canPlaySound)
            source.PlayOneShot(doorUsingSound);

        if (!isUsingByEnemy)
        {
            canUse = false;
            StartCoroutine(DoorUseDelay());
        }
        else
            isUsingByEnemy = false;
    }

    public IEnumerator ButtonBacklightBlink()
    {
        // when enemy is using door, door buttons blinks and plays beepSound
        float delay = 0.3f;
        isUsingByEnemy = true;

        while(true)
        {
            buttonBacklight.color = Color.red;
            source.PlayOneShot(beepSound);
            yield return new WaitForSeconds(delay);
            buttonBacklight.color = new Color(0.6226415f, 0.6226415f, 0.6226415f);
            yield return new WaitForSeconds(delay);
        }
    }

    public void OnEnemyUse()
    {
        // when enemy opens door
        // it isn't invokes directly, because returns error: Coroutine continue failure
        coroutine = StartCoroutine(ButtonBacklightBlink());
    }
}
