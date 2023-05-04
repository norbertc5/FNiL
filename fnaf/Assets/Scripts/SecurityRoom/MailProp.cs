using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailProp : MonoBehaviour
{
    // this script is responsible for that if player click on paper on desk, mail opens

    MailSystem mailSystem;

    void Start()
    {
        mailSystem = FindObjectOfType<MailSystem>();
    }

    private void OnMouseDown()
    {
        StartCoroutine(mailSystem.OpenMail());
    }
}
