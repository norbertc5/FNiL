using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecutityRoomLamp : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 2;

    void Update()
    {
        // rotate the lamp
        transform.eulerAngles += Vector3.up * rotationSpeed * Time.deltaTime;
    }
}
