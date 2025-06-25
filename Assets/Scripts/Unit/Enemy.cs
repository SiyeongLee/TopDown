using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    public float health = 1f;
    public float moveSpeed = 2f;
    public float attackRange = 1f;
    public float attackDamage = 0.1f;
    public IObjectPool<GameObject> pool { get; set; }

    private Transform player;
    private bool isDead = false;

    void Start()
    {
        Debug.Log($"[Enemy] Start 호출됨 - GameObject: {gameObject.name}, 위치: {transform.position}");
        
        // 플레이어 찾기 - 여러 방법 시도
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
        }
        if (playerObj == null)
        {
            // DungeonManager에서 플레이어 가져오기
            DungeonManager dungeonManager = DungeonManager.GetInstance();
            if (dungeonManager != null && dungeonManager.player != null)
            {
                playerObj = dungeonManager.player.gameObject;
            }
        }
        
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"[Enemy] 플레이어 찾음 - 플레이어 위치: {player.position}, 플레이어 이름: {player.name}");
        }
        else
        {
            Debug.LogError("[Enemy] 플레이어를 찾을 수 없습니다! Player 태그가 설정되어 있는지 확인하세요.");
        }

        Debug.Log($"[Enemy] 초기 상태 - 체력: {health}, 이동속도: {moveSpeed}, 공격범위: {attackRange}");
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (player == null)
        {
            // 플레이어를 다시 찾기
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            return;
        }

        // 플레이어 추적
        float dist = Vector2.Distance(transform.position, player.position);
        
        if (dist > attackRange)
        {
            // 이동
            Vector2 dir = (player.position - transform.position).normalized;
            Vector3 newPos = transform.position + (Vector3)dir * moveSpeed * Time.deltaTime;
            transform.position = newPos;
        }
        else
        {
            // 공격
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.TakeDamage(attackDamage);
                Debug.Log($"[Enemy] 플레이어 공격! 데미지: {attackDamage}");
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead)
        {
            Debug.LogWarning("[Enemy] 이미 사망한 적이 데미지를 받으려 함");
            return;
        }

        float oldHealth = health;
        health -= damage;
        
        Debug.Log($"[Enemy] 데미지 받음! 받은 데미지: {damage}, 이전 체력: {oldHealth}, 현재 체력: {health}");
        
        // 체력이 0 이하가 되면 즉시 사망
        if (health <= 0)
        {
            Debug.Log("[Enemy] 체력이 0 이하가 되어 사망 처리 시작");
            health = 0; // 체력을 0으로 확실히 설정
            Die();
        }
    }

    public virtual void SetStat(int difficulty)
    {
        Debug.Log($"[Enemy] SetStat 호출됨 - 난이도: {difficulty}");
        // 필요에 따라 구현
    }

    protected virtual void Die()
    {
        if (isDead)
        {
            Debug.LogWarning("[Enemy] 이미 사망 처리된 적이 Die()를 다시 호출");
            return;
        }

        isDead = true;
        Debug.Log($"[Enemy] 사망 처리 시작 - GameObject: {gameObject.name}, 위치: {transform.position}");

        // DungeonManager에 적 사망 알림 (도어 열기)
        DungeonManager dungeonManager = DungeonManager.GetInstance();
        if (dungeonManager != null)
        {
            dungeonManager.AddEnemy(-1);
            Debug.Log("[Enemy] DungeonManager.AddEnemy(-1) 호출됨 - 도어 열기 시도");
        }
        else
        {
            Debug.LogError("[Enemy] DungeonManager를 찾을 수 없습니다!");
        }

        // 무기 드랍 기능 제거 (WeaponManager 사용하지 않음)
        Debug.Log("[Enemy] 무기 드랍 기능 비활성화됨");

        // 풀링 처리
        if (pool != null)
        {
            Debug.Log("[Enemy] 풀로 반환");
            pool.Release(gameObject);
        }
        else
        {
            Debug.Log("[Enemy] 풀이 null이므로 Destroy로 제거");
            Destroy(gameObject);
        }
        
        Debug.Log("[Enemy] 사망 처리 완료");
    }

    void OnEnable()
    {
        Debug.Log($"[Enemy] OnEnable 호출됨 - GameObject: {gameObject.name}");
        isDead = false;
    }

    void OnDisable()
    {
        Debug.Log($"[Enemy] OnDisable 호출됨 - GameObject: {gameObject.name}");
    }

    void OnDestroy()
    {
        Debug.Log($"[Enemy] OnDestroy 호출됨 - GameObject: {gameObject.name}");
    }

    // 충돌 감지 추가
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Enemy] OnTriggerEnter2D 호출됨 - 충돌한 오브젝트: {other.gameObject.name}, 태그: {other.tag}, 레이어: {other.gameObject.layer}");
        
        // Bullet 컴포넌트로 충돌 감지
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet != null)
        {
            Debug.Log($"[Enemy] 총알과 충돌! 총알 데미지: {bullet.damage}");
            TakeDamage(bullet.damage);
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Enemy] OnCollisionEnter2D 호출됨 - 충돌한 오브젝트: {collision.gameObject.name}, 태그: {collision.gameObject.tag}, 레이어: {collision.gameObject.layer}");
        
        // Bullet 컴포넌트로 충돌 감지
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            Debug.Log($"[Enemy] 총알과 충돌! 총알 데미지: {bullet.damage}");
            TakeDamage(bullet.damage);
            return;
        }
    }

    // 수동으로 데미지를 받는 메서드 (디버깅용)
    [ContextMenu("테스트 데미지 받기")]
    public void TestTakeDamage()
    {
        Debug.Log("[Enemy] 테스트 데미지 받기 호출됨");
        TakeDamage(100f);
    }
}