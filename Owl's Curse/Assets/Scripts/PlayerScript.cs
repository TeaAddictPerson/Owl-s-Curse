using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody2D rd;
    private float HorizontalMove = 0f;
    private bool FacingRight = false;

    [Header("Здоровье")]
    public int maxHealth = 5;
    private int currentHealth;
    public float invincibilityDuration = 1f; 
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [Header("Атака")]
    public Transform attackPoint;
    public float AttackRange = 0.5f;
    public int attackDamage = 10;
    public LayerMask enemyLayers;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    [Header("Настройки передвижения гг")]
    [Range(0, 5f)] public float speed = 1f;
    [Range(0, 10f)] public float jump_force = 5f;

    public Animator animator;

    [Header("Проверка на заземление")]
    public bool IsGrounded = false;
    [Range(-5, 5f)] public float groundOffsetY = -1.8f;
    [Range(0, 10f)] public float groundRadius = 0.3f;

    private bool wasGrounded;
    public bool isDead = false;

    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        wasGrounded = IsGrounded;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

       
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
           
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            }
        }

      
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

     
        if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rd.AddForce(transform.up * jump_force, ForceMode2D.Impulse);
            animator.SetTrigger("Jump");
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

    public void TakeDamage(int damage)
    {
        Debug.Log($"Получен урон: {damage}. текущее хп: {currentHealth}");

        if (isInvincible || isDead)
        {
            Debug.Log("Дамаг не прошел игрок умер или в инвизе");
            return;
        }

        currentHealth -= damage;
        animator.SetTrigger("Hurt");

     
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

      
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);

        Debug.Log($"Хп после урона: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

      
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);

     
        animator.SetBool("IsDead", true);

        rd.isKinematic= true;
        rd.velocity = Vector2.zero;

    
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, AttackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
          
            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, AttackRange);
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            Vector2 targetVelocity = new Vector2(HorizontalMove * 10f, rd.velocity.y);
            rd.velocity = targetVelocity;
            CheckGround();
        }
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            new Vector2(transform.position.x, transform.position.y + groundOffsetY),
            groundRadius
        );

        IsGrounded = colliders.Length > 1;
    }


    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}