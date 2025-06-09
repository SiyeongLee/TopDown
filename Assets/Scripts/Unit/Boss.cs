using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SetStat( int difficulty )
    {
        maxHealth = defaultHealth;
        maxHealth *= difficulty * 3;
        health = maxHealth;
        healthBar.UpdateHealthBar( health, maxHealth );
    }

    public override void TakeDamage( float damage )
    {
        base.TakeDamage( damage );
    }
}
