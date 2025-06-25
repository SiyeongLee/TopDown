using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class WeaponData
{
    public int id;
    public string name;
    public string description;
    public float damage;
    public float fireRate;
    public float bulletSpeed;
    public int bulletCount;
    public Sprite weaponSprite;
    public Color weaponColor;
}

[System.Serializable]
public class WeaponDataList
{
    public List<WeaponData> weapons;
}

public class Weapon : MonoBehaviour
{
    [Header("무기 설정")]
    public GameObject muzzle;
    public GameObject bulletPrefab;
    public Text weaponNameText; // UI 텍스트 컴포넌트
    public Text weaponStatsText; // 무기 스탯 표시용 UI 텍스트
    
    [Header("무기 데이터")]
    public List<WeaponData> weaponList = new List<WeaponData>();
    
    private Quaternion weaponRotation;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private int currentWeaponId = 0;
    public Player player; // Inspector에서 할당
    
    public float lastFireTime { get; set; } = 0f;
    private string weaponDataJson;

    void Start()
    {
        Debug.Log("[Weapon] Start 호출됨");
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        // 무기 데이터 초기화
        InitializeWeaponData();
        
        // 무기를 0번(기본 권총)으로 고정
        currentWeaponId = 0;
        SetWeapon(currentWeaponId);
        
        Debug.Log($"[Weapon] 초기화 완료 - 고정 무기 ID: {currentWeaponId}");
    }

    void Update()
    {
        LookWeaponMouseCursor();
        HandleWeaponInput();
    }

    void InitializeWeaponData()
    {
        Debug.Log("[Weapon] 무기 데이터 초기화 시작");
        
        // 무기 1: 기본 권총
        WeaponData pistol = new WeaponData
        {
            id = 0,
            name = "기본 권총",
            description = "안정적이고 정확한 기본 권총",
            damage = 25f,
            fireRate = 0.5f,
            bulletSpeed = 20f,
            bulletCount = 1,
            weaponColor = Color.white
        };
        
        // 무기 2: 샷건
        WeaponData shotgun = new WeaponData
        {
            id = 1,
            name = "샷건",
            description = "근거리에서 강력한 샷건",
            damage = 15f,
            fireRate = 1.0f,
            bulletSpeed = 15f,
            bulletCount = 5,
            weaponColor = Color.red
        };
        
        // 무기 3: 라이플
        WeaponData rifle = new WeaponData
        {
            id = 2,
            name = "자동 라이플",
            description = "연사력이 뛰어난 자동 라이플",
            damage = 20f,
            fireRate = 0.2f,
            bulletSpeed = 25f,
            bulletCount = 1,
            weaponColor = Color.blue
        };
        
        weaponList.Add(pistol);
        weaponList.Add(shotgun);
        weaponList.Add(rifle);
        
        // JSON으로 직렬화
        WeaponDataList weaponDataList = new WeaponDataList { weapons = weaponList };
        weaponDataJson = JsonUtility.ToJson(weaponDataList, true);
        
        Debug.Log($"[Weapon] 무기 데이터 초기화 완료 - 무기 개수: {weaponList.Count}");
        Debug.Log($"[Weapon] JSON 데이터:\n{weaponDataJson}");
    }

    void HandleWeaponInput()
    {
        // 숫자키 무기 변경 기능 제거 - 무기를 한 개로 고정
        
        // 마우스 클릭으로 발사 (GetMouseButtonDown으로 변경)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Weapon] 마우스 클릭 감지 - 발사 시도");
            Fire();
        }
        
        // 연사 지원 (GetMouseButton 유지)
        if (Input.GetMouseButton(0))
        {
            Fire();
        }
    }

    public void LookWeaponMouseCursor()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        Vector2 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float z = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        weaponRotation = Quaternion.Euler(0, 0, z);
        transform.rotation = weaponRotation;
        
        if (90 <= z && z <= 180 || -180 <= z && z <= -90)
        {
            spriteRenderer.flipY = true;
        }
        else
        {
            spriteRenderer.flipY = false;
        }
    }

    public void Fire()
    {
        Debug.Log("[Weapon] Fire() 호출됨");
        
        if (currentWeaponId >= weaponList.Count)
        {
            Debug.LogError($"[Weapon] 잘못된 무기 ID: {currentWeaponId}");
            return;
        }
        
        WeaponData currentWeapon = weaponList[currentWeaponId];
        
        // 발사 속도 체크
        if (Time.time - lastFireTime < currentWeapon.fireRate)
        {
            Debug.Log($"[Weapon] 발사 쿨다운 중 - 남은 시간: {currentWeapon.fireRate - (Time.time - lastFireTime):F2}초");
            return;
        }
        
        lastFireTime = Time.time;
        
        Debug.Log($"[Weapon] 발사! 무기: {currentWeapon.name}, 데미지: {currentWeapon.damage}");
        
        // PoolingManager 확인
        if (PoolingManager.GetInstance() == null)
        {
            Debug.LogError("[Weapon] PoolingManager가 null입니다!");
            return;
        }
        
        // muzzle 확인
        if (muzzle == null)
        {
            Debug.LogError("[Weapon] muzzle이 null입니다! Inspector에서 할당하세요.");
            return;
        }
        
        // 총알 발사
        for (int i = 0; i < currentWeapon.bulletCount; i++)
        {
            GameObject bulletObj = PoolingManager.GetInstance().bulletPool.Get();
            if (bulletObj == null)
            {
                Debug.LogError("[Weapon] 총알 오브젝트를 가져올 수 없습니다!");
                continue;
            }
            
            bulletObj.transform.position = muzzle.transform.position;
            bulletObj.transform.rotation = weaponRotation;
            
            // 샷건의 경우 탄퍼짐 효과
            if (currentWeapon.bulletCount > 1)
            {
                float spreadAngle = UnityEngine.Random.Range(-15f, 15f);
                Quaternion spreadRotation = weaponRotation * Quaternion.Euler(0, 0, spreadAngle);
                bulletObj.transform.rotation = spreadRotation;
            }
            
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Fire(currentWeapon.bulletSpeed, bulletObj.transform.rotation, currentWeapon.damage);
                Debug.Log($"[Weapon] 총알 {i+1} 발사 완료 - 속도: {currentWeapon.bulletSpeed}, 데미지: {currentWeapon.damage}");
            }
            else
            {
                Debug.LogError("[Weapon] Bullet 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }

    public void SetWeapon(int weaponId)
    {
        if (weaponId >= 0 && weaponId < weaponList.Count)
        {
            currentWeaponId = weaponId;
            WeaponData weaponData = weaponList[weaponId];
            
            // 스프라이트 변경
            if (spriteRenderer != null)
            {
                spriteRenderer.color = weaponData.weaponColor;
            }
            
            // UI 업데이트
            UpdateWeaponUI(weaponData);
            
            Debug.Log($"[Weapon] 무기 변경: {weaponData.name}");
        }
        else
        {
            Debug.LogError($"[Weapon] 잘못된 무기 ID: {weaponId}");
        }
    }

    void UpdateWeaponUI(WeaponData weaponData)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponData.name;
        }
        
        if (weaponStatsText != null)
        {
            weaponStatsText.text = $"데미지: {weaponData.damage}\n발사속도: {weaponData.fireRate:F1}\n총알속도: {weaponData.bulletSpeed}";
        }
    }

    public WeaponData GetCurrentWeapon()
    {
        if (currentWeaponId >= 0 && currentWeaponId < weaponList.Count)
        {
            return weaponList[currentWeaponId];
        }
        return null;
    }

    public void SaveWeaponDataToJson()
    {
        WeaponDataList weaponDataList = new WeaponDataList { weapons = weaponList };
        string json = JsonUtility.ToJson(weaponDataList, true);
        Debug.Log($"[Weapon] 무기 데이터 JSON 저장:\n{json}");
    }

    public void LoadWeaponDataFromJson()
    {
        if (!string.IsNullOrEmpty(weaponDataJson))
        {
            WeaponDataList weaponDataList = JsonUtility.FromJson<WeaponDataList>(weaponDataJson);
            weaponList = weaponDataList.weapons;
            Debug.Log($"[Weapon] 무기 데이터 JSON 로드 완료 - 무기 개수: {weaponList.Count}");
        }
    }
    
    public int GetCurrentWeaponId()
    {
        return currentWeaponId;
    }
}
