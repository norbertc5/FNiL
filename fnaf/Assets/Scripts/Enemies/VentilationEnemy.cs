using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using System.IO;
using System.Linq;

public class VentilationEnemy : MonoBehaviour
{
    [Header("Objects references")]
    [Space(6)]

    [SerializeField] Animator gridAnim;
    [SerializeField] Transform lastTargetPosition;
    [SerializeField] GameObject enemyRenderer;
    [SerializeField] GameObject jumpscareEnemy;
    [SerializeField] CamerasController camerasController;
    [SerializeField] GameObject playerCameraObject;
    Animator anim;
    EnemyWalk enemyWalkScript;

    [Space(10)]
    [Header("Sounds")]
    [Space(6)]

    [SerializeField] AudioClip squeakSound;
    [SerializeField] AudioClip footstepSound;
    [SerializeField] AudioClip jumpscareSound;
    AudioSource source;

    [Space(10)]
    [Header("Settings")]
    [Space(6)]

    [SerializeField] int startHour;


    float timeToChangeState;  // time to change state of walk
    bool hasOpenedGrid;
    [HideInInspector] public bool canCountTimeToChangeState;
    bool canCountTimeToJumpscare;
    bool isWalking;
    float timeToJumpscare = 6;
    bool hasMadeJumpscare;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        enemyWalkScript = GetComponent<EnemyWalk>();
        source = GetComponent<AudioSource>();
        timeToChangeState = Random.Range(5, 20);
        GameManager.OnHourChanges += CheckHour;
        GameManager.OnConfigSet += SetUpVentEnemy;
    }

    void Update()
    {
        #region Walk states (open grid, walk)
        if (canCountTimeToChangeState)
            timeToChangeState -= Time.deltaTime;

        if(timeToChangeState <= 0)
        {
            if(!hasOpenedGrid)
            {
                // open grid
                gridAnim.enabled = true;
                hasOpenedGrid = true;
                GameManager.soundsSource.PlayOneShot(squeakSound);
                timeToChangeState = Random.Range(4, 16);
            }
            else
            {
                // start walk
                GetComponent<EnemyWalk>().enabled = true;
                StartCoroutine(WalkBreaks());
                StartCoroutine(PlayFootstepSound());
                isWalking = true;
                canCountTimeToChangeState = false;
                canCountTimeToJumpscare = true;
                timeToChangeState = 1;
            }
        }

        // when near to last target position, stops corutines
        if (Vector3.Distance(transform.position, lastTargetPosition.position) < 1)
        {
            isWalking = false;
            StopAllCoroutines();
        }
        #endregion

        #region Jumpscare
        // when player isn'timeToChangeState watching this enemy, timeToJumpscare will decrease
        if (canCountTimeToJumpscare && (CamerasController.areSecurityCamerasOpen || !IsEnemyVisible(Camera.main, enemyRenderer)))
            timeToJumpscare -= Time.deltaTime;

        // jumpscare
        if (timeToJumpscare <= 0 && !hasMadeJumpscare)
        {
            enemyRenderer.SetActive(false);
            StopAllCoroutines();
            StartCoroutine(PreparationToJumpscare());
            hasMadeJumpscare = true;
        }
        #endregion
    }

    IEnumerator WalkBreaks()
    {
        // wait some time and make breaks in walk
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8, 19));

            anim.CrossFade("Idle", 0.5f);
            enemyWalkScript.enabled = false;
            isWalking = false;

            yield return new WaitForSeconds(Random.Range(5, 12));

            anim.CrossFade("crawl", 0.5f);
            enemyWalkScript.enabled = true;
            isWalking = true;
        }
    }

    IEnumerator PlayFootstepSound()
    {
        // play footstep sound
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (isWalking)
                source.PlayOneShot(footstepSound);
        }
    }

    bool IsEnemyVisible(Camera c, GameObject target)
    {
        // if this enemy is watching by player
        // method taken from: https://www.youtube.com/watch?v=0IrZ3LDJoeM

        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        var point = target.transform.position;

        foreach(var plane in planes)
        {
            if(plane.GetDistanceToPoint(point) < 0)
                return false;
        }
        return true;
    }

    IEnumerator PreparationToJumpscare()
    {
        // make light blink before jumpscare
        StartCoroutine(GameManager.MianLightBlinking());
        yield return new WaitForSeconds(Random.Range(2, 5));
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.Jumpscare(enemyRenderer, jumpscareEnemy, jumpscareSound);
    }

    void CheckHour()
    {
        if (TimeSystem.hour == startHour)
            canCountTimeToChangeState = true;
    }

    void SetUpVentEnemy()
    {
        string pathToStartHour = Application.streamingAssetsPath + "/Configs" + "/Enemies" + "/Enemy4" + "/StartHour" + ".txt";
        List<string> startHourContent = File.ReadAllLines(pathToStartHour).ToList();
        startHour = int.Parse(startHourContent[GameManager.actualNightIndex - 1]);

        string pathToTimeToJumpscare = Application.streamingAssetsPath + "/Configs" + "/Enemies" + "/Enemy4" + "/timeToJumpscare" + ".txt";
        List<string> timeToJumpscareContent = File.ReadAllLines(pathToTimeToJumpscare).ToList();
        timeToJumpscare = float.Parse(timeToJumpscareContent[GameManager.actualNightIndex - 1]);
    }
}
