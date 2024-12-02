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

    public Transform attackPoint;
    public float AttackRange=0.5f;

    public int attackDamage=10;
    public LayerMask enemyLayers;

    public float attackRate=2f;
    float nextAttackTime=0f;
  

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
        if(Time.time >= nextAttackTime)
        {
                if(Input.GetMouseButtonDown(0))
                {
                     Attack();
                     nextAttackTime=Time.time+1f/attackRate;
                }
        }
    
        
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

      void Attack()
        {
            animator.SetTrigger("Attack");
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, AttackRange, enemyLayers);

            foreach(Collider2D enemy in hitEnemies)
            {
               enemy.GetComponent<Frog>().TakeDamage(attackDamage);
            }
        }

        void OnDrawGizmosSelected()
        {
            if(attackPoint == null)
            return;
            
           Gizmos.DrawWireSphere(attackPoint.position,AttackRange); 
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