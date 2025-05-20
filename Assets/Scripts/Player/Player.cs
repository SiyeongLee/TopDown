using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;


public class Player : MonoBehaviour

{
    
    

    [SerializeField] float moveSpeed = 2f;
    [SerializeField] Sprite spriteUp;
    [SerializeField] Sprite spriteDown;
    [SerializeField] Sprite spriteLeft;
    [SerializeField] Sprite spriteRight;
    
    Rigidbody2D rb;
    SpriteRenderer sR;

    Vector2 input;
    Vector2 velocity;
    int score;

    [SerializeField] TextMeshProUGUI scoreText;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sR= GetComponent<SpriteRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    private void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        velocity = input.normalized * moveSpeed;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x > 0)
                sR.sprite = spriteRight;
            else if (input.x < 0)
                sR.sprite = spriteLeft;
        }
        else
        {
            if(input.y > 0)
                sR.sprite = spriteUp;
            else
                sR.sprite = spriteDown;
        }
    }
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("item"))
        { 
         score += collision.GetComponent<itemObject>().GetPoint();
         scoreText.text = score.ToString();
            Destroy(collision.gameObject);
        }
    }





























}
