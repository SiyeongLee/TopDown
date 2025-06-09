using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    protected float health = 100;
    protected float maxHealth = 100;
    [SerializeField] protected float defaultHealth = 100;
    [SerializeField] protected HealthBar healthBar;

    public IObjectPool<GameObject> pool { get; set; }

    private void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public virtual void SetStat(int difficulty)
    {
        maxHealth = defaultHealth;
        maxHealth *= difficulty;
        health = maxHealth;
        healthBar.UpdateHealthBar( health, maxHealth );
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        healthBar.UpdateHealthBar( health, maxHealth );
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        PoolingManager.GetInstance().enemyPool.Release( gameObject );
        DungeonManager.GetInstance().AddEnemy( -1 );
    }
}
