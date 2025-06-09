using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerController))]
public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    private PlayerController controller;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigid;
    private Animator animator;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 moveInput = new Vector2( Input.GetAxisRaw( "Horizontal" ), Input.GetAxisRaw( "Vertical" ) );
        Vector2 moveVelocity = moveInput.normalized * speed;
        controller.Move( moveVelocity );
        MoveRendering();

        if (Input.GetButtonDown("Fire1"))
        {
            controller.Attack();
        }
    }

    void MoveRendering()
    {
        if (Input.GetAxisRaw( "Horizontal" ) > 0)
        {
            animator.SetBool( "isMove", true );
            spriteRenderer.flipX = false;
        }
        else if (Input.GetAxisRaw( "Horizontal" ) < 0)
        {
            animator.SetBool( "isMove", true );
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.SetBool( "isMove", false );
        }
    }

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }
}
