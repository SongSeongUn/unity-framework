using UnityEngine;
using System.Diagnostics;

public static class DebugUtils 
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(object m) => UnityEngine.Debug.Log(m);

    [Conditional("UNITY_EDITOR")]
    public static void LogError(object m) => UnityEngine.Debug.Log(m);

    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object m) => UnityEngine.Debug.LogWarning(m);
}