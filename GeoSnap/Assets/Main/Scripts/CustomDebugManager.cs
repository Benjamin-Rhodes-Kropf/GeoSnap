using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDebugManager : MonoBehaviour
{
    public static CustomDebugManager instance;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode;
    
    [Header("Colors")]
    [SerializeField] private Color buttonBgColor;
    [SerializeField] private Color hudForegroundColor;
}
