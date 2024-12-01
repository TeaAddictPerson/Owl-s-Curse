using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody2D rd;
    private float HorizontalMove = 0f;
    private bool FacingRight = false;
  

    [Header("Настройки передвижения гг")]
    [Range(0, 5f)] public float speed = 1f;
    [Range(0, 10f)] public float jump_force = 5f;

    public Animator animator;
    [Space]
    [Header("Проверка на заземление")]
    public bool IsGrounded = false;
    [Range(-5, 5f)] public float groundOffsetY = -1.8f;
    [Range(0, 10f)] public float groundRadius = 0.3f;

    private bool wasGrounded;
    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        bool wasGrounded = IsGrounded;
    }


  

    private void Update()
    {
        
        if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rd.AddForce(transform.up * jump_force, ForceMode2D.Impulse);
        }

       
        HorizontalMove = Input.GetAxisRaw("Horizontal") * speed;
        animator.SetFloat("HorizontalMove", Mathf.Abs(HorizontalMove));

       
        if (HorizontalMove < 0 && FacingRight)
        {
            Flip();
        }
        else if (HorizontalMove > 0 && !FacingRight)
        {
            Flip();
        }

        
        if (IsGrounded != wasGrounded) 
        {
            animator.SetBool("Jumping", !IsGrounded);
            wasGrounded = IsGrounded;
        }
    }


    private void FixedUpdate()
    {
        Vector2 targetVelocity = new Vector2(HorizontalMove * 10f, rd.velocity.y);
        rd.velocity = targetVelocity;
        CheckGround();
    }

    private void Flip()
    {
        FacingRight = !FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void CheckGround()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll
            (new Vector2(transform.position.x, transform.position.y + groundOffsetY), groundRadius);

        if (colliders.Length > 1)
        {
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
    }
}