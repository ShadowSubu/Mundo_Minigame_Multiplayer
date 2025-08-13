using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class OrientationTools
{
#if UNITY_EDITOR
    
    private static bool isPortrait = true;

    /// <summary>
    /// Toggle between Portrait and Landscape using Ctrl + Alt + O
    /// </summary>
    [MenuItem("Window/Orientation/Toggle Orientation %&O")] // % -> Ctrl, & -> Alt, O -> key
    public static void ToggleOrientation()
    {
        if (isPortrait)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Debug.Log("Set Orientation to Landscape Left");
        }
        else
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Debug.Log("Set Orientation to Portrait");
        }

        isPortrait = !isPortrait; // Toggle the value
    }

    /// <summary>
    /// Set Orientation to Landscape
    /// </summary>
    [MenuItem("Window/Orientation/Landscape")]
    public static void SetLandscapeOrientation()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        isPortrait = false;
        Debug.Log("Set Orientation to Landscape Left");
    }
    
    /// <summary>
    /// Set Orientation to Portrait
    /// </summary>
    [MenuItem("Window/Orientation/Portrait")]
    public static void SetPortraitOrientation()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        isPortrait = true;
        Debug.Log("Set Orientation to Portrait");
    }
    
#endif
}
