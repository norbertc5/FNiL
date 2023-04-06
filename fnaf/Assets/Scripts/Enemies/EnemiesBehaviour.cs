using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using System.Linq;

public class EnemiesBehaviour : MonoBehaviour
{
    [Header("Walk Setting")]
    [Space(6)]

    public GameObject[] allPossiblePositions;
    public Transform[] objectsWithAction;  // objects which will do any action when enemy moved
    public GameManager.Rooms[] enemysRooms;  // rooms where enemy is in each move
    public bool[] isOptionalRoom;
    [SerializeField] float[] changePositionDealyBorders;

    [HideInInspector] public int actualPosition;
    [HideInInspector] public int nextPosition = 1;  // next position (from allPossiblePositions) where enemy will be
    [SerializeField] bool isCorridorOccupedbyThis;
    [SerializeField] int startHour;

    [Space(10)]
    [Header("Things necessary\nfor jumpscare")]
    [Space(6)]

    public DoorButton doorScript;  // script of door to which enemy comes
    public LightCorridorButton lightCorridorScript;
    [SerializeField] GameObject playerCameraObject;
    [SerializeField] CamerasController cameraController;
    [SerializeField] AudioSource jumpscareSource;
    public AudioClip jumpscareSound;
    float randomMoveTime;

    [Space(10)]
    [Header("Other")]
    [Space(6)]

    [SerializeField] bool canOpenSecurityDoor;
    [SerializeField] bool canChangePath;

    void Start()
    {
        GameManager.OnHourChanges += StartWalking;
        GameManager.OnConfigSet += SetUpEnemy;
        ChangePathIfPossible();
    }


    public IEnumerator ChangePosition()
    {
        // waits random time set in changePositionDealyBorders array and change enemys position
        int randomNumber = 0;
        StopCoroutine(OnLastPosition());

        while (true) 
        {
            randomMoveTime = Random.Range(changePositionDealyBorders[0], changePositionDealyBorders[1]);

            if (allPossiblePositions[actualPosition].GetComponent<EnemyWalk>())
                yield return new WaitForSeconds(15);
            else
                yield return new WaitForSeconds(randomMoveTime);

            ChangePosDelayBorders(Random.Range(-1f, -2f));
            GameManager.OnEnemyPositionChanged?.Invoke();

            if(canChangePath)
                GetComponent<EnemyPathChanger>().EnemiesInFrontOfDoorsDissapar();

            #region Security cameras glitch
            // invokes when any of enemies moved
            // when player is looking at camera on which enemy moved, starts CameraGlitched
            if (CamerasController.securityCameras[CamerasController.actualSecurityCam].GetComponent<SecurityCameras>().room
            == enemysRooms[actualPosition]
                || CamerasController.securityCameras[CamerasController.actualSecurityCam].GetComponent<SecurityCameras>().room
            == enemysRooms[nextPosition])
            {
                if(!CamerasController.areSecurityCamsGlitched && cameraController.gameObject.activeSelf)
                    StartCoroutine(cameraController.CameraGlitched());
            }
            #endregion

            #region Only 1 enemy in form of security room door
            // if other enemy is near security room door (on same side like this enemy) this enemy won'timeToChangeState move until other is near door
            if ((enemysRooms[enemysRooms.Length - 2] == GameManager.Rooms.RightCorridor && !GameManager.isRightCorridorOccuped) 
                || (GameManager.isRightCorridorOccuped && isCorridorOccupedbyThis)
                || (enemysRooms[enemysRooms.Length - 2] == GameManager.Rooms.LeftCorridor && !GameManager.isLeftCorridorOccuped)
                || (GameManager.isLeftCorridorOccuped && isCorridorOccupedbyThis))
            {
                allPossiblePositions[actualPosition].SetActive(false);
                allPossiblePositions[nextPosition].SetActive(true);
                actualPosition = nextPosition;

                if (nextPosition < allPossiblePositions.Length - 2)
                    nextPosition++;
            }

            if (enemysRooms[actualPosition] == GameManager.Rooms.LeftCorridor && !GameManager.isLeftCorridorOccuped)
            {
                isCorridorOccupedbyThis = true;
                GameManager.isLeftCorridorOccuped = true;
            }
            if (enemysRooms[actualPosition] == GameManager.Rooms.RightCorridor && !GameManager.isRightCorridorOccuped)
            {
                isCorridorOccupedbyThis = true;
                GameManager.isRightCorridorOccuped = true;
            }
            #endregion

            #region Moving to optional rooms
            // move enemy to random rooms
            if (isOptionalRoom[nextPosition])
            {
                randomNumber = Random.Range(1, 3);

                if (randomNumber == 2)
                    nextPosition += 1;
            }
            #endregion

            #region Make action with objects on map
            // make action with object set in objectsWithAction array
            if (objectsWithAction[actualPosition] != null)
            {
                if (!isOptionalRoom[actualPosition] || (isOptionalRoom[actualPosition] && randomNumber == 1))
                {
                    objectsWithAction[actualPosition].GetComponent<EnvironmentChanges>().ApplyAction();
                }
            }
            #endregion

            #region Enemy in fornt of security room
            // when enemy is in fornt of scurity room entrance
            if (actualPosition == allPossiblePositions.Length - 2)
            {
                StartCoroutine(OnLastPosition());                
            }
            #endregion
        }
    }

    IEnumerator OnLastPosition()
    {
        yield return new WaitForSeconds(randomMoveTime);

        // if door is closed
        if(doorScript.isOn)
        {
            // there is 1 in 3 chance that enemy who can open door, do this
            if (Random.Range(1, 4) == 1 && canOpenSecurityDoor)
            {
                doorScript.UseDoor(0.1f, true, false);
                doorScript.OnEnemyUse();
            }            
            else
            {
                nextPosition = Random.Range(5, 10);
                ChangePosDelayBorders(Random.Range(4f, 6f));

                if (lightCorridorScript.isOn)
                    StartCoroutine(lightCorridorScript.LightCorridorBlink());

                // detect if security room door is free
                if (enemysRooms[enemysRooms.Length - 2] == GameManager.Rooms.LeftCorridor)
                    GameManager.isLeftCorridorOccuped = false;
                if (enemysRooms[enemysRooms.Length - 2] == GameManager.Rooms.RightCorridor)
                    GameManager.isRightCorridorOccuped = false;

                isCorridorOccupedbyThis = false;
                ChangePathIfPossible();
            }
        }
        else
        {
            // jumpscare
            SetPosDealyBorders(1, 1);  // to avoid situation that enemy is visible in corridor before jumpscare
            nextPosition = allPossiblePositions.Length - 1;
            GameManager gameManager = FindObjectOfType<GameManager>();
            gameManager.Jumpscare(allPossiblePositions[actualPosition], allPossiblePositions[nextPosition], jumpscareSound);
            StopAllCoroutines();
        }
    }

    void ChangePathIfPossible()
    {
        if (canChangePath)
        {
            GetComponent<EnemyPathChanger>().TryChangePath();
        }
    }

    void StartWalking()
    {
        // start walkin on appointed hour
        if (TimeSystem.hour == startHour && this.gameObject.activeSelf)
            StartCoroutine(ChangePosition());
    }

    void ChangePosDelayBorders(float changeBy)
    {
        // enemy moves faster when close to player
        // to it work, lower border must be greater than 5
        if (changePositionDealyBorders[0] + changeBy > 5)
        {
            changePositionDealyBorders[0] += changeBy;
            changePositionDealyBorders[1] += changeBy;
        }
    }

    public void SetPosDealyBorders(float upBorderSetTo, float downBorderSetTo)
    {
        // set changePositionDealyBorders to specific values 
        changePositionDealyBorders[0] = upBorderSetTo;
        changePositionDealyBorders[1] = downBorderSetTo;
    }

    void SetUpEnemy()
    {
        int enemyIndex = System.Array.IndexOf(GameManager.enemies, this) + 1;

        #region StartHour set
        string pathToStartHour = Application.streamingAssetsPath + "/Configs" + "/Enemies" + "/Enemy" + enemyIndex + "/StartHour" + ".txt";
        List<string> startHourContent = File.ReadAllLines(pathToStartHour).ToList();
        startHour = int.Parse(startHourContent[GameManager.actualNightIndex - 1]);
        #endregion

        #region ChangePosDelayBorders set
        string pathToChangePosDelay = Application.streamingAssetsPath + "/Configs" + "/Enemies" + "/Enemy" + enemyIndex + "/ChangePosDelayBorders" + ".txt";
        List<string> changePosDelayContent = File.ReadAllLines(pathToChangePosDelay).ToList();
        string[] splitedLine = changePosDelayContent[GameManager.actualNightIndex - 1].Split("-");

        changePositionDealyBorders[0] = int.Parse(splitedLine[0]);
        changePositionDealyBorders[1] = int.Parse(splitedLine[1]);
        #endregion
    }
}
