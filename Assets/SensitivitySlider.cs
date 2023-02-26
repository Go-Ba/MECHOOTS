using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] SettingsObject settings;
    void Start()
    {
        slider.value = settings.sensitivity;
    }
    void Update()
    {
        settings.sensitivity = slider.value;
    }
}
