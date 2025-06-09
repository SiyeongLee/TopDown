using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolingManager : MonoBehaviour
{
    private static PoolingManager instance;

    public int defaultCapacity = 100;
    public int maxPoolSize = 100;
    public IObjectPool<GameObject> enemyPool { get; private set; }
    public GameObject enemyPrefab;

    public IObjectPool<GameObject> bulletPool { get; private set; }
    public GameObject bulletPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy( gameObject );
        }

        Init();
    }

    public static PoolingManager GetInstance()
    {
        return instance;
    }

    private void Init()
    {
        enemyPool = new ObjectPool<GameObject>( CreatePooledItemEnemy, OnTakeFromPoolEnemy, OnReturnedToPoolEnemy, OnDestroyPoolObjectEnemy, true, defaultCapacity, maxPoolSize );
        
        for (int i = 0; i < defaultCapacity; i++)
        {
            Enemy enemy = CreatePooledItemEnemy().GetComponent<Enemy>();
            enemy.pool.Release( enemy.gameObject );
        }

        bulletPool = new ObjectPool<GameObject>( CreatePooledItemBullet, OnTakeFromPoolBullet, OnReturnedToPoolBullet, OnDestroyPoolObjectBullet, true, defaultCapacity, maxPoolSize );

        for (int i = 0; i < defaultCapacity; i++)
        {
            Bullet bullet = CreatePooledItemBullet().GetComponent<Bullet>();
            bullet.pool.Release( bullet.gameObject );
        }
    }

    // 생성
    private GameObject CreatePooledItemEnemy()
    {
        GameObject poolGo = Instantiate( enemyPrefab );
        poolGo.GetComponent<Enemy>().pool = this.enemyPool;
        return poolGo;
    }

    // 사용
    private void OnTakeFromPoolEnemy( GameObject poolGo )
    {
        poolGo.SetActive( true );
    }

    // 반환
    private void OnReturnedToPoolEnemy( GameObject poolGo )
    {
        poolGo.SetActive( false );
    }

    // 삭제
    private void OnDestroyPoolObjectEnemy( GameObject poolGo )
    {
        Destroy( poolGo );
    }

    // 생성
    private GameObject CreatePooledItemBullet()
    {
        GameObject poolGo = Instantiate( bulletPrefab );
        poolGo.GetComponent<Bullet>().pool = this.bulletPool;
        return poolGo;
    }

    // 사용
    private void OnTakeFromPoolBullet( GameObject poolGo )
    {
        poolGo.SetActive( true );
    }

    // 반환
    private void OnReturnedToPoolBullet( GameObject poolGo )
    {
        poolGo.SetActive( false );
    }

    // 삭제
    private void OnDestroyPoolObjectBullet( GameObject poolGo )
    {
        Destroy( poolGo );
    }
}
