using System;
using UnityEngine;

public class EditorLog
{
    private EditorLog(){}
    
#if UNITY_EDITOR
    public static void Log(String message)
    {
        Debug.Log(message);
    }
    
    public static void Warning(String message)
    {
        Debug.LogWarning(message);
    }
    
    public static void Error(String message)
    {
        Debug.LogError(message);
    }
#endif
}
