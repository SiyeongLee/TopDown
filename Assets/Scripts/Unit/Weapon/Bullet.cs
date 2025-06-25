using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    public float damage = 40f;
    public float lifeTime = 10f;
    private float speed;
    private Vector3 direction;
    public IObjectPool<GameObject> pool { get; set; }
    private bool isReleased = false;

    public void Fire(float bulletSpeed, Quaternion rotation, float damage)
    {
        speed = bulletSpeed;
        direction = rotation * Vector3.right;
        this.damage = damage;
        Invoke("DestroySelf", lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isReleased) return;
        Debug.Log($"[Bullet] OnTriggerEnter2D 충돌: {collision.gameObject.name}, 태그: {collision.tag}, 레이어: {collision.gameObject.layer}");
        
        // Enemy 컴포넌트로 충돌 감지
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"[Bullet] Enemy 컴포넌트 찾음! 데미지 전달: {damage}");
            enemy.TakeDamage(damage);
            
            // 즉시 파괴
            DestroySelf();
            return;
        }
        
        // Player 컴포넌트 확인 (플레이어에게는 데미지 주지 않음)
        if (collision.GetComponent<Player>() != null)
        {
            Debug.Log("[Bullet] Player와 충돌 - 무시");
        }
        // 기타 오브젝트
        else
        {
            Debug.Log($"[Bullet] 기타 오브젝트와 충돌: {collision.gameObject.name}");
        }
        
        DestroySelf();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isReleased) return;
        Debug.Log($"[Bullet] OnCollisionEnter2D 충돌: {collision.gameObject.name}, 태그: {collision.gameObject.tag}, 레이어: {collision.gameObject.layer}");
        
        // Enemy 컴포넌트로 충돌 감지
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"[Bullet] Enemy 컴포넌트 찾음! 데미지 전달: {damage}");
            enemy.TakeDamage(damage);
            
            // 즉시 파괴
            DestroySelf();
            return;
        }
        
        DestroySelf();
    }

    void DestroySelf()
    {
        if (isReleased) return;
        isReleased = true;

        if (pool != null)
        {
            pool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        isReleased = false;
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}