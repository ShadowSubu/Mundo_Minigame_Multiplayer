#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class JingleOnCompile
{
    [DidReloadScripts(10)]
    public static void Jingle()
    {
        AudioClip ac = Resources.Load<AudioClip>("JingleOnCompile");
        // Debug.Log(ac.name);
        PlayClip(ac);
    }
    
    public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
      
        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );

        // Debug.Log(method);
        method.Invoke(
            null,
            new object[] { clip, startSample, loop }
        );
    }

    public static void StopAllClips()
    {
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

        Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        MethodInfo method = audioUtilClass.GetMethod(
            "StopAllPreviewClips",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new Type[] { },
            null
        );

        Debug.Log(method);
        method.Invoke(
            null,
            new object[] { }
        );
    }
}

#endif