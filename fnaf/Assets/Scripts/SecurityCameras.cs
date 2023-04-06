using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCameras : MonoBehaviour
{
    // this script is responsible for security cams behviour

    [Header("Main")]
    [Space(6)]

    [SerializeField] CamerasController camerasController;
    [SerializeField] Color noctovisionColor;
    [SerializeField] Light noctovisionLight;
    [SerializeField] int deafultLightIntensity;
    [SerializeField] int deafultLightRange;
    [HideInInspector] public bool isUsingLight;
    AudioSource source;
    public GameManager.Rooms room;  // room at which camera is looking
                                    // using to make glitch when enemy's moved on actual security camera
    [Space(10)]
    [Header("Rotating")]
    [Space(6)]

    [SerializeField] bool canRotate;
    [SerializeField] float rotateSpeed;
    [SerializeField] float startRotation;
    [SerializeField] float targetRotation;
    enum CameraRotationDirections  {Left, Right}
    [SerializeField] CameraRotationDirections rotation;
    bool isRotating = true;
    bool freezeSpeed = true;  // make rotateSpeed can'timeToChangeState be changed for animationToPlay moment

    private void OnEnable()
    {
        noctovisionLight.intensity = deafultLightIntensity;
        noctovisionLight.range = deafultLightRange;
        source = GetComponent<AudioSource>();

        if (canRotate)
        {
            if (rotation == CameraRotationDirections.Right)
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, startRotation, transform.eulerAngles.z);
            else
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetRotation, transform.eulerAngles.z);

            StartCoroutine(HoldRotating());
            StartCoroutine(PlayCamerasSound());
        }

    }

    private void OnDisable()
    {
        if(isUsingLight)
        {
            Battery.ChangePowerUsage(Battery.powerUsage -= 1);
            camerasController.UpdateBatteryUsageUI();
            noctovisionLight.intensity = deafultLightIntensity;
            noctovisionLight.range = deafultLightRange;
            isUsingLight = false;
            StopAllCoroutines();
        }
    }

    void Update()
    {
        // when player press Z on keyboard, light'll make stronger
        if(Input.GetKeyDown(KeyCode.Z))
        {
            Battery.ChangePowerUsage(Battery.powerUsage += 1);
            camerasController.UpdateBatteryUsageUI();
            noctovisionLight.intensity *= 5;
            noctovisionLight.range *= 5;
            isUsingLight = true;
        }
        if (Input.GetKeyUp(KeyCode.Z) && isUsingLight)
        {
            Battery.ChangePowerUsage(Battery.powerUsage -= 1);
            camerasController.UpdateBatteryUsageUI();
            noctovisionLight.intensity = deafultLightIntensity;
            noctovisionLight.range = deafultLightRange;
            isUsingLight = false;
        }

        // make that camera can rotates
        if(canRotate)
        {
            if(isRotating)
                transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

            // rotating is working diffrent when targetRotation is bigger than 360 and when it's lower than 360
            if (targetRotation < 360)
            {
                if (transform.eulerAngles.y > targetRotation && freezeSpeed)
                {
                    rotateSpeed = -rotateSpeed;
                    StartCoroutine(HoldRotating());
                    freezeSpeed = false;
                }

                if(transform.eulerAngles.y < startRotation)
                    StartCoroutine(HoldRotating());
            }
            else
            {
                if (transform.eulerAngles.y < (targetRotation - 360))
                    rotateSpeed = -rotateSpeed;

                if(transform.eulerAngles.y > (targetRotation - 360) && transform.eulerAngles.y < (targetRotation - 360))
                    StartCoroutine(HoldRotating());
                if (transform.eulerAngles.y < startRotation && transform.eulerAngles.y > (targetRotation - 360))
                    StartCoroutine(HoldRotating());
            }

            if (transform.eulerAngles.y < startRotation && transform.eulerAngles.y < 360)
            {
                freezeSpeed = true;
                rotateSpeed *= -1;
            }
        }
    }


    /// <summary>
    /// Make rotating camera stops, waits and keep on rotating
    /// </summary>
    IEnumerator HoldRotating()
    {
        // make rotating camera stops, waits and keep on rotating
        isRotating = false;
        yield return new WaitForSeconds(0.6f);
        isRotating = true;
    }

    IEnumerator PlayCamerasSound()
    {
        // play sound while camera is rotating and pause it when camera holds
        source.Play();

        while(true)
        {
            if (isRotating)
                source.UnPause();
            else
                source.Pause();

            yield return null;
        }
    }
}