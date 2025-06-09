using UnityEngine;
using Object = UnityEngine.Object;

/**
 * 2024.03.02
 * @조민재, 최초 작성
 */

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool _shutdown = false;
    private static object _lock = new Object();
    
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_shutdown)
            {
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    if (_instance == null)
                    {
                        var go = new GameObject();
                        _instance = go.AddComponent<T>();
                        go.name = typeof(T).ToString() + " (Singleton)";
                    
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
    }

    #region >>>> Unity

    private void OnApplicationQuit()
    {
        _shutdown = true;
    }

    private void OnDestroy()
    {
        _shutdown = true;
    }

    #endregion
}
