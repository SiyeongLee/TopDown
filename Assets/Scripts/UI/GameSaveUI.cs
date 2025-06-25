using UnityEngine;
using UnityEngine.UI;
using System;

public class GameSaveUI : MonoBehaviour
{
    [Header("UI 버튼들")]
    public Button saveButton;
    public Button loadButton;
    public Button clearSaveButton;
    public Button autoSaveToggleButton;
    
    [Header("정보 표시")]
    public Text infoText;
    public Text autoSaveStatusText;
    
    [Header("키보드 단축키")]
    public KeyCode saveKey = KeyCode.F5;
    public KeyCode loadKey = KeyCode.F9;
    
    private GameSaveSystem saveSystem;
    private bool autoSaveEnabled = true;
    
    void Start()
    {
        saveSystem = GameSaveSystem.GetInstance();
        
        if (saveSystem == null)
        {
            Debug.LogError("[GameSaveUI] GameSaveSystem을 찾을 수 없습니다!");
            return;
        }
        
        // 버튼 이벤트 설정
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveGame);
        
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadGame);
        
        if (clearSaveButton != null)
            clearSaveButton.onClick.AddListener(ClearSaveData);
        
        if (autoSaveToggleButton != null)
            autoSaveToggleButton.onClick.AddListener(ToggleAutoSave);
        
        UpdateUI();
    }
    
    void Update()
    {
        // 키보드 단축키 처리
        if (Input.GetKeyDown(saveKey))
        {
            SaveGame();
        }
        
        if (Input.GetKeyDown(loadKey))
        {
            LoadGame();
        }
        
        // 실시간으로 정보 업데이트
        UpdateInfoText();
    }
    
    void SaveGame()
    {
        if (saveSystem != null)
        {
            saveSystem.SaveGame();
            Debug.Log("[GameSaveUI] 수동 저장 완료");
            UpdateUI();
        }
    }
    
    void LoadGame()
    {
        if (saveSystem != null)
        {
            GameSaveData saveData = saveSystem.LoadGame();
            
            // 데이터 로드
            saveSystem.LoadPlayerData(saveData.playerData);
            saveSystem.LoadWeaponData(saveData.weaponData);
            saveSystem.LoadProgressData(saveData.progressData);
            
            Debug.Log("[GameSaveUI] 수동 로드 완료");
            UpdateUI();
        }
    }
    
    void ClearSaveData()
    {
        if (saveSystem != null)
        {
            saveSystem.ClearSaveData();
            Debug.Log("[GameSaveUI] 저장 데이터 삭제 완료");
            UpdateUI();
        }
    }
    
    void ToggleAutoSave()
    {
        if (saveSystem != null)
        {
            autoSaveEnabled = !autoSaveEnabled;
            saveSystem.autoSave = autoSaveEnabled;
            UpdateAutoSaveStatus();
            Debug.Log($"[GameSaveUI] 자동 저장 {(autoSaveEnabled ? "활성화" : "비활성화")}");
        }
    }
    
    void UpdateUI()
    {
        UpdateInfoText();
        UpdateAutoSaveStatus();
    }
    
    void UpdateInfoText()
    {
        if (infoText != null && saveSystem != null)
        {
            string info = $"저장 파일 존재: {(saveSystem.HasSaveData() ? "예" : "아니오")}\n";
            info += $"저장 파일 경로:\n{saveSystem.GetSaveFilePath()}\n\n";
            info += $"저장 파일 정보:\n{saveSystem.GetSaveFileInfo()}\n\n";
            
            // 플레이어 정보
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                info += $"플레이어 정보:\n";
                info += $"위치: {player.transform.position}\n";
                info += $"체력: {player.health}/{player.maxHealth}\n";
                info += $"이동속도: {player.moveSpeed}\n";
                info += $"무적상태: {(player.isInvincible ? "예" : "아니오")}\n";
            }
            
            // 무기 정보
            Weapon weapon = FindObjectOfType<Weapon>();
            if (weapon != null)
            {
                info += $"\n무기 정보:\n";
                info += $"현재 무기 ID: {weapon.GetCurrentWeaponId()}\n";
            }
            
            // 던전 정보
            DungeonManager dungeonManager = DungeonManager.GetInstance();
            if (dungeonManager != null)
            {
                info += $"\n던전 정보:\n";
                info += $"현재 방 ID: {dungeonManager.playerRoomID}\n";
                info += $"난이도: {dungeonManager.difficulty}\n";
                info += $"적 수: {dungeonManager.enemyCount}\n";
                info += $"방문한 방 수: {dungeonManager.isRoomVisited.Count}\n";
            }
            
            infoText.text = info;
        }
    }
    
    void UpdateAutoSaveStatus()
    {
        if (autoSaveStatusText != null)
        {
            string status = $"자동 저장: {(autoSaveEnabled ? "활성화" : "비활성화")}\n";
            status += $"단축키 - 저장: {saveKey}, 로드: {loadKey}";
            autoSaveStatusText.text = status;
        }
    }
    
    // 버튼 활성화/비활성화
    public void SetSaveButtonInteractable(bool interactable)
    {
        if (saveButton != null)
            saveButton.interactable = interactable;
    }
    
    public void SetLoadButtonInteractable(bool interactable)
    {
        if (loadButton != null)
            loadButton.interactable = interactable;
    }
    
    // 저장 파일 존재 여부에 따라 버튼 상태 업데이트
    public void UpdateButtonStates()
    {
        if (saveSystem != null)
        {
            bool hasSaveData = saveSystem.HasSaveData();
            SetLoadButtonInteractable(hasSaveData);
            
            if (clearSaveButton != null)
                clearSaveButton.interactable = hasSaveData;
        }
    }
} 