using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float health = 10000f;
    public float maxHealth = 10000f;
    public Weapon weapon; // Inspector에서 할당
    public float attackDamage = 50f;
    public int currentWeaponId = 0;
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;
    Rigidbody2D rb;
    Vector2 input;
    Vector2 velocity;
    private SpriteRenderer sR;
    
    // 무적시간 관련 변수
    public bool isInvincible = false;
    private float invincibleTime = 2f; // 무적시간 2초로 단축
    public float invincibleTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine; // 깜빡임 코루틴 참조

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sR = GetComponent<SpriteRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log("[Player] Awake 호출됨");
    }

    private void Start()
    {
        Debug.Log("[Player] Start 호출됨");
        
        // 무적시간 시작
        StartInvincibility();
        
        // 무기 정보 로드
        currentWeaponId = PlayerPrefs.GetInt("LastWeaponId", 0);
        
        // Weapon 컴포넌트가 있으면 무기 설정
        if (weapon != null)
        {
            weapon.SetWeapon(currentWeaponId);
        }
        else
        {
            Debug.LogWarning("[Player] Weapon 컴포넌트가 할당되지 않았습니다!");
        }
        
        Debug.Log($"[Player] 초기화 완료 - 현재 무기 ID: {currentWeaponId}");
    }

    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        
        // 입력이 있으면 이동
        if (input.sqrMagnitude > 0.01f)
        {
            velocity = input.normalized * moveSpeed;
            
            // 방향에 따라 스프라이트 변경
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                if (input.x > 0 && spriteRight != null)
                    sR.sprite = spriteRight;
                else if (input.x < 0 && spriteLeft != null)
                    sR.sprite = spriteLeft;
            }
            else
            {
                if (input.y > 0 && spriteUp != null)
                    sR.sprite = spriteUp;
                else if (input.y < 0 && spriteDown != null)
                    sR.sprite = spriteDown;
            }
        }
        else
        {
            velocity = Vector2.zero;
        }

        // 무적시간 업데이트
        UpdateInvincibility();

        // 디버그 로그 추가
        if (input.sqrMagnitude > 0.01f)
        {
            Debug.Log($"[Player] 입력 감지: {input}, 속도: {velocity}");
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    // 무적시간 시작
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibleTimer = invincibleTime;
        Debug.Log($"[Player] 무적시간 시작! {invincibleTime}초 동안 무적");
        
        // 무적 시 시각적 효과 (깜빡임)
        blinkCoroutine = StartCoroutine(InvincibilityBlink());
    }

    // 무적시간 업데이트
    private void UpdateInvincibility()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log("[Player] 무적시간 종료!");
                
                // 스프라이트 정상화
                if (spriteRenderer != null)
                {
                    Color normalColor = spriteRenderer.color;
                    normalColor.a = 1f;
                    spriteRenderer.color = normalColor;
                }
                
                // 깜빡임 코루틴 중지
                StopCoroutine(blinkCoroutine);
            }
        }
    }

    // 무적 시 깜빡임 효과
    private System.Collections.IEnumerator InvincibilityBlink()
    {
        while (isInvincible && invincibleTimer > 0f)
        {
            if (spriteRenderer != null)
            {
                // 반투명
                Color blinkColor = spriteRenderer.color;
                blinkColor.a = 0.5f;
                spriteRenderer.color = blinkColor;
            }
            yield return new WaitForSeconds(0.1f);
            
            if (spriteRenderer != null && isInvincible && invincibleTimer > 0f)
            {
                // 완전 투명
                Color blinkColor = spriteRenderer.color;
                blinkColor.a = 0.2f;
                spriteRenderer.color = blinkColor;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        // 무적시간 종료 시 스프라이트 정상화
        if (spriteRenderer != null)
        {
            Color normalColor = spriteRenderer.color;
            normalColor.a = 1f;
            spriteRenderer.color = normalColor;
        }
    }

    public void TakeDamage(float damage)
    {
        // 무적 상태면 데미지 무시
        if (isInvincible)
        {
            Debug.Log($"[Player] 무적 상태! 데미지 무시: {damage}");
            return;
        }

        health -= damage;
        Debug.Log($"[Player] 데미지 받음! 받은 데미지: {damage}, 남은 체력: {health}");
        
        // 체력이 0 이하가 되어도 죽지 않음 - 무적 상태로 유지
        if (health <= 0)
        {
            health = 1f; // 체력을 1로 유지
            Debug.Log("[Player] 체력이 0 이하가 되었지만 무적 상태로 유지됩니다!");
            
            // 무적시간 재시작
            StartInvincibility();
        }
    }

    private void Die()
    {
        Debug.Log("[Player] 사망 처리 시작 - 하지만 실제로는 죽지 않습니다!");
        
        // 플레이어는 죽지 않음 - 체력을 1로 복원하고 무적 상태로 유지
        health = 1f;
        StartInvincibility();
        
        Debug.Log("[Player] 체력 복원 및 무적 상태로 유지됨");
    }

    public void SetWeapon(int weaponId)
    {
        Debug.Log($"[Player] SetWeapon 호출됨 - 무기 ID: {weaponId}");
        
        currentWeaponId = weaponId;
        
        // Weapon 컴포넌트를 통해 무기 설정
        if (weapon != null)
        {
            weapon.SetWeapon(weaponId);
            // Weapon 스크립트에서 attackDamage를 설정하므로 여기서는 제거
        }
        else
        {
            Debug.LogError("[Player] Weapon 컴포넌트가 null입니다! Inspector에서 할당하세요.");
        }
        
        PlayerPrefs.SetInt("LastWeaponId", weaponId);
        PlayerPrefs.Save();
        
        Debug.Log($"[Player] 무기 변경 완료 - 무기 ID: {weaponId}");
    }

    // 충돌 감지 (적과 닿아도 즉시 죽지 않음)
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Player] OnTriggerEnter2D - 충돌한 오브젝트: {other.gameObject.name}");
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log("[Player] 적과 충돌! 하지만 즉시 죽지 않습니다.");
            // 적과 충돌해도 TakeDamage를 호출하지 않음 (무적시간 동안은 완전 무시)
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Player] OnCollisionEnter2D - 충돌한 오브젝트: {collision.gameObject.name}");
        
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log("[Player] 적과 충돌! 하지만 즉시 죽지 않습니다.");
            // 적과 충돌해도 TakeDamage를 호출하지 않음
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 충돌 처리 단순화 - 벽과의 충돌 시에도 이동 차단하지 않음
        Debug.Log($"[Player] OnCollisionStay2D - 충돌한 오브젝트: {collision.gameObject.name}");
    }
}