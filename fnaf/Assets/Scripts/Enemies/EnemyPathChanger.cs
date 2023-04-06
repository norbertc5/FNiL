using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPathChanger : MonoBehaviour
{
    EnemiesBehaviour enemyController;

    // alt = alternative, org = orginal
    [SerializeField] int changePathFromStep;

    // alternative settings of enenmy's path
    [SerializeField] GameObject[] altPossiblePositions;
    [SerializeField] Transform[] altObjectsWithAction;
    [SerializeField] GameManager.Rooms[] altEnemysRooms;
    [SerializeField] bool[] altOptionalRoom;
    [SerializeField] DoorButton altDoorScript;
    [SerializeField] LightCorridorButton altLightCorridorScrpit;

    // orginal settings of enemy's path
    GameObject[] orgPossiblePositions;
    Transform[] orgObjectsWithAction;
    GameManager.Rooms[] orgEnemysRooms;
    bool[] orgOptionalRoom;
    DoorButton orgDoorScript;
    LightCorridorButton orgLightCorridorScrpit;

    void Awake()
    {
        enemyController = GetComponent<EnemiesBehaviour>();

        orgPossiblePositions = enemyController.allPossiblePositions;
        orgObjectsWithAction = enemyController.objectsWithAction;
        orgEnemysRooms = enemyController.enemysRooms;
        orgOptionalRoom = enemyController.isOptionalRoom;

        orgDoorScript = enemyController.doorScript;
        orgLightCorridorScrpit = enemyController.lightCorridorScript;
    }

    public void TryChangePath()
    {
        // make that enemy may change his path to player

        /*
        // turn off enemy when he changes path. Using to remove enemy positions from old path
        // condition is to avoid turn off all enemy's postitions with start pos on game start
        */

        // if random number = 1 path stays orginal, else changes to alternative
        if (UnityEngine.Random.Range(1, 3) == 1)
        {
            // orginal path
            enemyController.allPossiblePositions = orgPossiblePositions;
            enemyController.objectsWithAction = orgObjectsWithAction;
            enemyController.enemysRooms = orgEnemysRooms;
            enemyController.isOptionalRoom = orgOptionalRoom;
            enemyController.doorScript = orgDoorScript;
            enemyController.lightCorridorScript = orgLightCorridorScrpit;
        }
        else
        {
            // alternative path
            enemyController.allPossiblePositions = altPossiblePositions;
            enemyController.objectsWithAction = altObjectsWithAction;
            enemyController.enemysRooms = altEnemysRooms;
            enemyController.isOptionalRoom = altOptionalRoom;
            enemyController.doorScript = altDoorScript;
            enemyController.lightCorridorScript = altLightCorridorScrpit;
        }        
    }

    public void EnemiesInFrontOfDoorsDissapar()
    {
        // when path is changing, enemies in fron of doors disappears
        orgPossiblePositions[orgPossiblePositions.Length - 2].SetActive(false);
        altPossiblePositions[altPossiblePositions.Length - 2].SetActive(false);
    }
}
