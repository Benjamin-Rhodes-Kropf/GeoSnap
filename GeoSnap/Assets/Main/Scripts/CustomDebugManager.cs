using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDebugManager : MonoBehaviour
{
    public static CustomDebugManager instance;
    
    [Header("Debug")]
    public bool debugMode;
    public bool showDebugButtons;
    
    [Header("Colors")]
    [SerializeField] private Color buttonBgColor;
    [SerializeField] private Color hudForegroundColor;
}
