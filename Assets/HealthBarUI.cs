using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Slider slider;
    private void OnEnable()
    {
        if (slider == null) slider = GetComponent<Slider>();
        PlayerController.onHealthUpdated += SetValue;
    }
    private void OnDisable()
    {
        PlayerController.onHealthUpdated -= SetValue;
    }

    void SetValue(float _val)
    {
        slider.value = _val;
    }
}
