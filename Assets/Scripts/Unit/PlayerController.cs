using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector2 velocity;
    Rigidbody2D rigid;
    Weapon weapon;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        weapon = transform.Find("Weapon").gameObject.GetComponent<Weapon>();
    }

    void FixedUpdate()
    {
        rigid.MovePosition( rigid.position + velocity * Time.fixedDeltaTime );
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Attack()
    {
        weapon.Fire();
    }
}
