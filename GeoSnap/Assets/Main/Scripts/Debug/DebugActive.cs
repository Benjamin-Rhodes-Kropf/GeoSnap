using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugActive : MonoBehaviour
{ 
    private void Awake()
    {
        if (CustomDebugManager.instance.showDebugButtons)
        {
            gameObject.GetComponent<GameObject>().active =true;
        }
        else
        {
            gameObject.GetComponent<GameObject>().active =false;
        }
    }

    private void Update()
    {
        
    }
}
