using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTestManager : MonoBehaviour
{
    private static GameTestManager instance;

    public bool allMapVisibleMode;
    public bool DoNotMoveCameraMode;

    private void Awake()
    {
        // ½Ì±ÛÅæ ÆÐÅÏ Àû¿ë
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );
        }
    }

    public static GameTestManager GetInstance()
    {
        return instance;
    }
}
