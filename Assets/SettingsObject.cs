using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Settings Object")]
public class SettingsObject : ScriptableObject
{
    public AspectRatio aspectRatio;
    public bool autoRun;
    public int FoV;
    public float sensitivity;
}
