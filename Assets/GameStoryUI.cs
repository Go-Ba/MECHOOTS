using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameStoryUI : MonoBehaviour
{
    public UnityEvent onSpacePressed;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            onSpacePressed.Invoke();
    }
}
