using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    TextMeshProUGUI fpsCounterText;
    int frames;

    private void Start()
    {
        fpsCounterText = GetComponent<TextMeshProUGUI>();
        InvokeRepeating("CountFPS", 1, 1);
    }

    void CountFPS()
    {
        frames = (int)(1 / Time.unscaledDeltaTime);
        fpsCounterText.text = frames + " FPS";

    }
}
