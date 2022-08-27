using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    // animator that controls transitions, requires next and prev triggers
    public Animator ScreenAnimator;

    // containers for currently displayed screen and hidden screens
    public Transform activeParent;
    public Transform inactiveParent;
    
    // containers for animating screens
    public Transform startParent;
    public Transform endParent;
    
    // screens to be dislayed
    public UIScreen[] Screens;

    public List<UIScreen> history;
    UIScreen current;

    // index in Screens of the first screen to be displayed
    public int startScreenIndex = 0;
    public int loadingScreenIndex = 0;

    void Start()
    {
        // re-parent all screen transforms to hidden object
        foreach(var s in Screens)
        {
            s.ScreenObject.gameObject.SetActive(true);
            s.ScreenObject.transform.SetParent(inactiveParent, false);
        }
        
        ResetMenu();
        activeParent.gameObject.SetActive(true);
        inactiveParent.gameObject.SetActive(false);
    }
    public void ChangeScreen(string ScreenID)
    {
        UIScreen screen = ScreenFromID(ScreenID);
        if ( screen != null)
        {
            history.Clear();
            ScreenAnimator.SetTrigger("Next"); // trigger animation
            current.ScreenObject.SetParent(startParent, false); // set current screen parent for animation
            history.Add(current); // add current screen to history
            current = screen; // assign new as current
            screen.ScreenObject.SetParent(endParent, false); // set new screen parent for animation
        }
    }
    
    //Todo: Make work for more than one screen
    public void GoBackOneScreen()
    {
        if (history.Count < 1) { 
            Debug.LogWarning("historyLessThanOne");
            return; // if first screen, ignore
        }
        UIScreen screen = history[history.Count - 1]; // get previous screen
        history.Remove(history[history.Count - 1]); // remove current screen from history
        ScreenAnimator.SetTrigger("Next"); // trigger animation
        current.ScreenObject.SetParent(startParent, false); // set current screen parent for animation
        current = screen; // assign old as current
        screen.ScreenObject.SetParent(endParent, false); // set old screen parent for animation
    }
    
    UIScreen ScreenFromID(string ScreenID)
    {
        foreach (UIScreen screen in Screens)
        {
            if (screen.Name == ScreenID) return screen;
        }

        return null;
    }
    public void SetActiveParent()
    {
        // hide inactive screens
        foreach (var s in Screens)
        {
            if (s != current) s.ScreenObject.SetParent(inactiveParent, false);
        }

        // show active screen
        current.ScreenObject.SetParent(activeParent, false);
    }
    public void ResetMenu()
    {
        // clear history
        history = new List<UIScreen>();
        
        //set loading screen
        UIScreen screen = ScreenFromID("LoadingScreen");
        current = screen; // set start screen
        current.ScreenObject.SetParent(startParent, false); // set current screen parent for animation

        //old code
        //set screen to load in
        // ScreenAnimator.SetTrigger("Next");  // trigger animation
        // current = Screens[startScreenIndex]; // set start screen
        // current.ScreenObject.SetParent(endParent, false); // set start screen parent for animation
    }

    //backForTestMode
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (history.Count > 0)
            {
                //Todo: get history working (look at how rqts did it)
                GoBackOneScreen();
            }
        }
    }
}

[System.Serializable]
public class UIScreen
{
    public string Name;
    public Transform ScreenObject;
}