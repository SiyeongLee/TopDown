using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager GetInstance() => instance;
    
    [Header("게임 설정")]
    public bool loadSaveDataOnStart = true;
    public bool showSaveUI = true;
    
    private GameSaveSystem saveSystem;
    private GameSaveUI saveUI;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        saveSystem = GameSaveSystem.GetInstance();
        saveUI = FindObjectOfType<GameSaveUI>();
        
        if (saveSystem == null)
        {
            Debug.LogError("[GameManager] GameSaveSystem을 찾을 수 없습니다!");
            return;
        }
        
        // 게임 시작 시 저장된 데이터 로드
        if (loadSaveDataOnStart)
        {
            LoadGameOnStart();
        }
        
        Debug.Log("[GameManager] 게임 매니저 초기화 완료");
    }
    
    /// <summary>
    /// 게임 시작 시 저장된 데이터 로드
    /// </summary>
    void LoadGameOnStart()
    {
        if (saveSystem.HasSaveData())
        {
            Debug.Log("[GameManager] 저장된 데이터 발견 - 게임 로드 시작");
            
            GameSaveData saveData = saveSystem.LoadGame();
            
            // 데이터 로드
            saveSystem.LoadPlayerData(saveData.playerData);
            saveSystem.LoadWeaponData(saveData.weaponData);
            saveSystem.LoadProgressData(saveData.progressData);
            
            Debug.Log("[GameManager] 게임 시작 시 저장 데이터 로드 완료");
        }
        else
        {
            Debug.Log("[GameManager] 저장된 데이터가 없습니다 - 새 게임 시작");
        }
    }
    
    /// <summary>
    /// 게임 저장
    /// </summary>
    public void SaveGame()
    {
        if (saveSystem != null)
        {
            saveSystem.SaveGame();
            Debug.Log("[GameManager] 게임 저장 완료");
        }
    }
    
    /// <summary>
    /// 게임 로드
    /// </summary>
    public void LoadGame()
    {
        if (saveSystem != null)
        {
            GameSaveData saveData = saveSystem.LoadGame();
            
            // 데이터 로드
            saveSystem.LoadPlayerData(saveData.playerData);
            saveSystem.LoadWeaponData(saveData.weaponData);
            saveSystem.LoadProgressData(saveData.progressData);
            
            Debug.Log("[GameManager] 게임 로드 완료");
        }
    }
    
    /// <summary>
    /// 새 게임 시작 (저장 데이터 삭제)
    /// </summary>
    public void StartNewGame()
    {
        if (saveSystem != null)
        {
            saveSystem.ClearSaveData();
            Debug.Log("[GameManager] 새 게임 시작 - 저장 데이터 삭제");
        }
        
        // 씬 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame()
    {
        // 게임 종료 전 저장
        SaveGame();
        
        Debug.Log("[GameManager] 게임 종료");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 저장 UI 토글
    /// </summary>
    public void ToggleSaveUI()
    {
        if (saveUI != null)
        {
            saveUI.gameObject.SetActive(!saveUI.gameObject.activeSelf);
        }
    }
    
    // 게임 종료 시 자동 저장
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
            Debug.Log("[GameManager] 게임 일시정지 - 자동 저장");
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame();
            Debug.Log("[GameManager] 게임 포커스 해제 - 자동 저장");
        }
    }
    
    void OnApplicationQuit()
    {
        SaveGame();
        Debug.Log("[GameManager] 게임 종료 - 자동 저장");
    }
} 