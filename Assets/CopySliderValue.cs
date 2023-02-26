using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopySliderValue : MonoBehaviour
{

    [SerializeField] Slider slider;
    [SerializeField] TMP_Text text;
    [SerializeField] int decimalPlaces;
    void Update()
    {
        var pow = Mathf.Pow(10, decimalPlaces);
        var val = Mathf.Round(slider.value * pow) / pow;
        var s = val.ToString();
        if (text.text != s)
            text.text = s;
    }
}
