using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject muzzle;
    Camera mainCamera;
    SpriteRenderer spriteRenderer;
    Quaternion weaponRotation;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = DungeonManager.GetInstance().mainCamera;
    }

    void Update()
    {
        LookWeaponMouseCursor();
    }

    public void LookWeaponMouseCursor()
    {
        if (mainCamera == null)
        {
            mainCamera = DungeonManager.GetInstance().mainCamera;
        }
        Vector2 pos = mainCamera.ScreenToWorldPoint( Input.mousePosition ) - transform.position;
        float z = Mathf.Atan2( pos.y, pos.x ) * Mathf.Rad2Deg;
        weaponRotation = Quaternion.Euler( 0, 0, z );
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
        Bullet bullet = PoolingManager.GetInstance().bulletPool.Get().GetComponent<Bullet>();
        bullet.transform.position = muzzle.transform.position;
        bullet.transform.rotation = weaponRotation;
        bullet.Fire();
    }
}
