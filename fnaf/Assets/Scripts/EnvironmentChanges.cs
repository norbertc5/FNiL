using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentChanges : MonoBehaviour
{
    enum ActionType {ChangeRotation, PlayAnimation, ChangeObject, OnlySound};

    [Header("General")]
    [Space(6)]

    [SerializeField] ActionType thisObjectAction;
    [SerializeField] int nightToPlayAt = 1;  // only works if waitRandomTime is true, if ApplyAction is invoked by other script, it doesn't matter
    [SerializeField] bool waitRandomTime;
    [SerializeField] bool playOnlyOneTimeInGame;  // if true, sound will play one time in whole game

    [Space(10)]
    [Header("Rotation change")]
    [Space(6)]
    [SerializeField] Vector3 newRotation;

    [Space(10)]
    [Header("Play animation")]
    [Space(6)]
    [SerializeField] AnimationClip animationToPlay;
    [SerializeField] int numberOfRepeats = 1;

    [Space(10)]
    [Header("Change")]
    [Space(6)]
    [SerializeField] GameObject toChange;
    [SerializeField] GameObject newObject;
    [SerializeField] int cameraToMakeGlitch;  // if player is watching to this camera index, glitch'll apperas 

    [Space(10)]
    [Header("Sound")]
    [Space(6)]
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip clip;

    private IEnumerator Start()
    {
        // it's coroutine due to GameManager manage to set actualNightIndex variable (it's also it Start)
        yield return new WaitForSeconds(1);
        if (waitRandomTime && GameManager.actualNightIndex == nightToPlayAt)
            StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(Random.Range(35, 300));
        ApplyAction();
    }

    public void ApplyAction()
    {
        // if playOnlyOneTimeInGame true, action will be apply only once, even when manually invoked in other script
        if (!playOnlyOneTimeInGame || (playOnlyOneTimeInGame && PlayerPrefs.GetString("HasChangeMade " + this.name) != "yes"))
        {
            switch (thisObjectAction)
            {
                case ActionType.ChangeRotation: transform.eulerAngles = newRotation; break;
                case ActionType.PlayAnimation: StartCoroutine(PlayAnim()); break;
                case ActionType.ChangeObject: ChangeObject(); break;
            }

            // play sound when action is applying
            if (source != null && clip != null)
                source.PlayOneShot(clip);

            // action can be made only once in game
            if (playOnlyOneTimeInGame)
            {
                PlayerPrefs.SetString("HasChangeMade " + this.name, "yes");
                PlayerPrefs.Save();
            }
        }
    }

    IEnumerator PlayAnim()
    {
        // play anim and repeat it numberOfRepeats (variable) times
        GetComponent<Animator>().enabled = true;
        float animLenght = animationToPlay.length;
        int repeats = 0;

        // repeating
        while(true)
        {
            yield return new WaitForSeconds(animLenght);
            repeats++;

            if (repeats == numberOfRepeats)
            {
                GetComponent<Animator>().enabled = false;
            }
        }
    }

    void ChangeObject()
    {
        // change object to other one, invokes camera glitch if player is watching on change
        if (CamerasController.actualSecurityCam == cameraToMakeGlitch)
        {
            CamerasController camerasController = FindObjectOfType<CamerasController>();
            StartCoroutine(camerasController.CameraGlitched());
        }

        toChange.SetActive(false);
        newObject.SetActive(true);
    }
}
