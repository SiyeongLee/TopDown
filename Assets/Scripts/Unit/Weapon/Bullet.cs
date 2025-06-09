using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    bool isFire = false;
    float time = 3.0f;
    float curTime = 0f;
    [SerializeField] private float speed = 10;
    [SerializeField] private float damage = 40;
    CircleCollider2D circleCollider;

    public IObjectPool<GameObject> pool { get; set; }

    void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        if (isFire)
        {
            float x = Mathf.Sin( (transform.rotation.z + 90) * Mathf.Deg2Rad );
            float y = Mathf.Cos( (transform.rotation.z + 90) * Mathf.Deg2Rad );
            //float z = Mathf.Tan( transform.rotation.z * Mathf.Deg2Rad );
            Vector3 dir = new Vector3(x, y).normalized;
            this.transform.Translate( dir * Time.deltaTime * speed );
        }
    }

    public void Fire()
    {
        isFire = true;
        StartCoroutine( StartTimer() );
    }

    IEnumerator StartTimer()
    {
        curTime = time;
        while (curTime > 0)
        {
            curTime -= Time.deltaTime;
            yield return null;

            if (curTime <= 0)
            {
                curTime = 0;
                isFire = false;
                pool.Release( gameObject );
                yield break;
            }
        }
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        Boss boss = collision.gameObject.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage( damage );
        } 
        else if (enemy != null)
        {
            enemy.TakeDamage( damage );
        }

        if (gameObject.activeSelf)
            pool.Release( gameObject );
    }
}
