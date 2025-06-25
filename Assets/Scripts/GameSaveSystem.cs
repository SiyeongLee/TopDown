using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public float health;
    public float maxHealth;
    public int currentWeaponId;
    public float moveSpeed;
    public bool isInvincible;
    public float invincibleTimer;
}

[System.Serializable]
public class WeaponSaveData
{
    public int currentWeaponId;
    public List<WeaponData> ownedWeapons;
    public float lastFireTime;
}

[System.Serializable]
public class GameProgressSaveData
{
    public int playerRoomID;
    public int difficulty;
    public int enemyCount;
    public HashSet<int> visitedRooms;
    public bool isBossRoomVisited;
    public float playTime;
    public int enemiesKilled;
    public int roomsCleared;
}

[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData playerData;
    public WeaponSaveData weaponData;
    public GameProgressSaveData progressData;
    public DateTime saveTime;
    public string gameVersion;
    
    public GameSaveData()
    {
        playerData = new PlayerSaveData();
        weaponData = new WeaponSaveData();
        progressData = new GameProgressSaveData();
        saveTime = DateTime.Now;
        gameVersion = Application.version;
    }
}

public class GameSaveSystem : MonoBehaviour
{
    private static GameSaveSystem instance;
    public static GameSaveSystem GetInstance() => instance;
    
    [Header("저장 설정")]
    public string saveFileName = "game_save.json";
    public bool autoSave = true;
    public float autoSaveInterval = 60f; // 60초마다 자동 저장
    
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);
    private float lastAutoSaveTime = 0f;
    
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
    
    private void Update()
    {
        // 자동 저장
        if (autoSave && Time.time - lastAutoSaveTime > autoSaveInterval)
        {
            SaveGame();
            lastAutoSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 게임 데이터를 JSON으로 저장
    /// </summary>
    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = new GameSaveData();
            
            // 플레이어 데이터 저장
            SavePlayerData(saveData.playerData);
            
            // 무기 데이터 저장
            SaveWeaponData(saveData.weaponData);
            
            // 게임 진행 데이터 저장
            SaveProgressData(saveData.progressData);
            
            // JSON으로 변환
            string jsonData = JsonUtility.ToJson(saveData, true);
            
            // 파일에 저장
            File.WriteAllText(SaveFilePath, jsonData);
            
            Debug.Log($"[GameSaveSystem] 게임 저장 완료 - 파일: {SaveFilePath}");
            Debug.Log($"[GameSaveSystem] 저장된 데이터 크기: {jsonData.Length} 문자");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameSaveSystem] 게임 저장 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// JSON에서 게임 데이터 로드
    /// </summary>
    public GameSaveData LoadGame()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.Log("[GameSaveSystem] 저장 파일이 없습니다. 새 게임을 시작합니다.");
                return new GameSaveData();
            }
            
            // 파일에서 JSON 읽기
            string jsonData = File.ReadAllText(SaveFilePath);
            
            // JSON을 객체로 변환
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
            
            Debug.Log($"[GameSaveSystem] 게임 로드 완료 - 파일: {SaveFilePath}");
            Debug.Log($"[GameSaveSystem] 로드된 데이터 크기: {jsonData.Length} 문자");
            
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameSaveSystem] 게임 로드 실패: {e.Message}");
            return new GameSaveData();
        }
    }
    
    /// <summary>
    /// 플레이어 데이터 저장
    /// </summary>
    private void SavePlayerData(PlayerSaveData playerData)
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            playerData.position = player.transform.position;
            playerData.health = player.health;
            playerData.maxHealth = player.maxHealth;
            playerData.moveSpeed = player.moveSpeed;
            playerData.isInvincible = player.isInvincible;
            playerData.invincibleTimer = player.invincibleTimer;
            
            Debug.Log($"[GameSaveSystem] 플레이어 데이터 저장 - 위치: {playerData.position}, 체력: {playerData.health}");
        }
    }
    
    /// <summary>
    /// 무기 데이터 저장
    /// </summary>
    private void SaveWeaponData(WeaponSaveData weaponData)
    {
        Weapon weapon = FindObjectOfType<Weapon>();
        if (weapon != null)
        {
            weaponData.currentWeaponId = weapon.GetCurrentWeaponId();
            weaponData.lastFireTime = weapon.lastFireTime;
            
            // 보유 무기 목록 저장 (Weapon 스크립트에 ownedWeapons가 있다면)
            // weaponData.ownedWeapons = weapon.GetOwnedWeapons();
            
            Debug.Log($"[GameSaveSystem] 무기 데이터 저장 - 현재 무기 ID: {weaponData.currentWeaponId}");
        }
    }
    
    /// <summary>
    /// 게임 진행 데이터 저장
    /// </summary>
    private void SaveProgressData(GameProgressSaveData progressData)
    {
        DungeonManager dungeonManager = DungeonManager.GetInstance();
        if (dungeonManager != null)
        {
            progressData.playerRoomID = dungeonManager.playerRoomID;
            progressData.difficulty = dungeonManager.difficulty;
            progressData.enemyCount = dungeonManager.enemyCount;
            progressData.visitedRooms = dungeonManager.isRoomVisited;
            
            Debug.Log($"[GameSaveSystem] 진행 데이터 저장 - 방 ID: {progressData.playerRoomID}, 난이도: {progressData.difficulty}");
        }
    }
    
    /// <summary>
    /// 플레이어 데이터 로드
    /// </summary>
    public void LoadPlayerData(PlayerSaveData playerData)
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.transform.position = playerData.position;
            player.health = playerData.health;
            player.maxHealth = playerData.maxHealth;
            player.moveSpeed = playerData.moveSpeed;
            player.isInvincible = playerData.isInvincible;
            player.invincibleTimer = playerData.invincibleTimer;
            
            Debug.Log($"[GameSaveSystem] 플레이어 데이터 로드 - 위치: {playerData.position}, 체력: {playerData.health}");
        }
    }
    
    /// <summary>
    /// 무기 데이터 로드
    /// </summary>
    public void LoadWeaponData(WeaponSaveData weaponData)
    {
        Weapon weapon = FindObjectOfType<Weapon>();
        if (weapon != null)
        {
            weapon.SetWeapon(weaponData.currentWeaponId);
            weapon.lastFireTime = weaponData.lastFireTime;
            
            Debug.Log($"[GameSaveSystem] 무기 데이터 로드 - 현재 무기 ID: {weaponData.currentWeaponId}");
        }
    }
    
    /// <summary>
    /// 게임 진행 데이터 로드
    /// </summary>
    public void LoadProgressData(GameProgressSaveData progressData)
    {
        DungeonManager dungeonManager = DungeonManager.GetInstance();
        if (dungeonManager != null)
        {
            dungeonManager.playerRoomID = progressData.playerRoomID;
            dungeonManager.difficulty = progressData.difficulty;
            dungeonManager.enemyCount = progressData.enemyCount;
            dungeonManager.isRoomVisited = progressData.visitedRooms;
            
            Debug.Log($"[GameSaveSystem] 진행 데이터 로드 - 방 ID: {progressData.playerRoomID}, 난이도: {progressData.difficulty}");
        }
    }
    
    /// <summary>
    /// 저장 파일 삭제 (초기화)
    /// </summary>
    public void ClearSaveData()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[GameSaveSystem] 저장 데이터 삭제 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameSaveSystem] 저장 데이터 삭제 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 저장 파일 존재 여부 확인
    /// </summary>
    public bool HasSaveData()
    {
        return File.Exists(SaveFilePath);
    }
    
    /// <summary>
    /// 저장 파일 경로 반환
    /// </summary>
    public string GetSaveFilePath()
    {
        return SaveFilePath;
    }
    
    /// <summary>
    /// 저장 파일 정보 반환
    /// </summary>
    public string GetSaveFileInfo()
    {
        if (File.Exists(SaveFilePath))
        {
            FileInfo fileInfo = new FileInfo(SaveFilePath);
            return $"파일 크기: {fileInfo.Length} bytes, 수정 시간: {fileInfo.LastWriteTime}";
        }
        return "저장 파일이 없습니다.";
    }
} 