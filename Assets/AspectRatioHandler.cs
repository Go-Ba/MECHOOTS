using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatioHandler : MonoBehaviour
{
    [SerializeField] SettingsObject settings;
    void Update()
    {
        HandleResolution();
    }
    void HandleResolution()
    {/*
        if (Input.GetKeyDown(KeyCode.Alpha1))
            settings.aspectRatio = AspectRatio.A4_3;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            settings.aspectRatio = AspectRatio.A16_9;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            settings.aspectRatio = AspectRatio.A21_9;

        if (settings.aspectRatio == AspectRatio.A4_3 && (Screen.currentResolution.width != 640 || Screen.currentResolution.width != 480))
            Screen.SetResolution(640, 480, FullScreenMode.FullScreenWindow);
        if (settings.aspectRatio == AspectRatio.A16_9 && (Screen.currentResolution.width != 640 || Screen.currentResolution.width != 360))
            Screen.SetResolution(640, 360, FullScreenMode.FullScreenWindow);
        if (settings.aspectRatio == AspectRatio.A21_9 && (Screen.currentResolution.width != 840 || Screen.currentResolution.width != 360))
            Screen.SetResolution(840, 360, FullScreenMode.FullScreenWindow);*/
    }
}
