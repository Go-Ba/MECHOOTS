using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.ComponentModel;

public class PopupText : MonoBehaviour
{
    static PopupText instance;
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject parent;

    private void Awake()
    {
        if (instance != this)
        {
            if (instance != null)
                Destroy(instance);
            instance = this;
        }
    }
    public static void SetPopupText(string _text)
    {
        instance.text.text = _text;
        instance.parent.SetActive(true);
    }
    public static void DisablePopupText()
    {
        instance.parent.SetActive(false);
    }
}
