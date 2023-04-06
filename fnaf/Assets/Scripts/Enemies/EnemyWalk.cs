using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    // enemy (set in allPossiblePositions[] in EnemyBehaviour) with this script can walk
    // enemy should also has animator with walk animation

    [SerializeField] float speed = 1;
    [SerializeField] Transform[] targetPositions;
    [SerializeField] int rotationSpeedDivider = 10;  // to controll rotation speed
    float t;
    int actualPosition;
    int nextPosition = 1;


    void Update()
    {
        t += Time.deltaTime * speed;

        transform.position = Vector3.Lerp(targetPositions[actualPosition].position,
            targetPositions[nextPosition].position, t / 10);

        // smoothly rotate in the next target direction
        if (Quaternion.LookRotation(targetPositions[nextPosition].position - transform.position) != Quaternion.identity)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(targetPositions[nextPosition].position - transform.position), t / rotationSpeedDivider);
        }

        // when enemy reaches target
        if (Vector3.Distance(transform.position, targetPositions[nextPosition].position) < 0.1f
            && actualPosition < targetPositions.Length - 2)
        {
            t = 0;
            actualPosition++;
            nextPosition++;
        }
    }
}