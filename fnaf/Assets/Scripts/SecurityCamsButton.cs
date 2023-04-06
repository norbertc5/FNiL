using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class SecurityCamsButton : MonoBehaviour
{
    // this script is responsible for securiti cams buttons when player is unsing security cams

    [SerializeField] int buttonID;
    [SerializeField] Color deafutlColor;
    [SerializeField] Color blinkingColor;
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] string roomName;
    Image buttonsBackground;

    void Start()
    {
        buttonsBackground = transform.Find("Background").GetComponent<Image>();
        GameManager.OnSecurityCamChanged += StartBlinking;

        // make that right button is blinking on first time when player opens security cams
        if (buttonID == CamerasController.actualSecurityCam)
            StartBlinking();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    void StartBlinking()
    {
        // when button clicked, button starts blinking

        if (CamerasController.actualSecurityCam == buttonID && this.gameObject.activeSelf)
        {
            StartCoroutine(ActualSecuiryCamBlinking());
            SetRoomNameText();
        }
        else
        {
            StopAllCoroutines();
            buttonsBackground.color = deafutlColor;
        }
    }

    IEnumerator ActualSecuiryCamBlinking()
    {
        // blinking
        while (true)
        {
            buttonsBackground.color = blinkingColor;
            yield return new WaitForSeconds(1);
            buttonsBackground.color = deafutlColor;
            yield return new WaitForSeconds(1);
        }
    }

    void SetRoomNameText()
    {
        // set roomNameText to name of room, where camera is
        // also put spaces where it's necessary (ahead of Uppercase letter)

        var name = CamerasController.securityCameras[buttonID].GetComponent<SecurityCameras>().room.ToString();
        int additionalSpaces = 2;  // to work properly with more than 1 uppercase
        StringBuilder sb = new StringBuilder(name.Length + additionalSpaces);

        for (int i = 0; i < name.Length; i++)
        {
            if (Char.IsUpper(name[i]))
                sb.Append(" ");

            sb.Append(name[i]);
        }
        roomNameText.text = sb.ToString();
    }
}
