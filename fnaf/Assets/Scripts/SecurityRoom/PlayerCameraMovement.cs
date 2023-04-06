using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraMovement : MonoBehaviour
{
    // make player can rotate camera by keybard and mouse

    [SerializeField] float cameraRotationSpeed = 2;
    [SerializeField] float cameraRotationEdge;
    float cameraYRotation;
    float mouseX;

    void Update()
    {
        // get input from keyboard
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        mouseX = Input.mousePosition.x;

         if (mouseX <= 200 || horizontalInput == -1)
             cameraYRotation -= 1 * cameraRotationSpeed * Time.deltaTime;
         else if (mouseX > 200 && mouseX < 500)
             cameraYRotation -= 1 * cameraRotationSpeed / 3 * Time.deltaTime;

         if (mouseX >= Screen.width - 200 || horizontalInput == 1)
             cameraYRotation += 1 * cameraRotationSpeed * Time.deltaTime;
         else if (mouseX < Screen.width - 200 && mouseX > Screen.width - 500)
             cameraYRotation += 1 * cameraRotationSpeed / 3 * Time.deltaTime;

        cameraYRotation = Mathf.Clamp(cameraYRotation, -cameraRotationEdge, cameraRotationEdge);
         transform.eulerAngles = new Vector3(0, cameraYRotation);
    }
}