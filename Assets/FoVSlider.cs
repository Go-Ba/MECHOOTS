using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FoVSlider : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] SettingsObject settings;
    void Start()
    {
        slider.value = settings.FoV;
    }
    void Update()
    {
        settings.FoV = (int)slider.value;
    }
}
