using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    private Rigidbody2D rd;
    private float HorizontalMove = 0f;
    private bool FacingRight = false;

    public Image Bar;

    [Header("Здоровье")]
    public int maxHealth = 15;
    public int currentHealth;
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
    public Transform legs;
    public LayerMask Ground;

    public bool isDead = false;

    [Header("Настройки скольжения")]
    public float dashForce = 20f;        
    public float dashDuration = 0.2f;   
    public float dashCooldown = 1f;    
    private bool canDash = true;         
    private bool isDashing = false;      
    private float dashCooldownTimer = 0f;

    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

        IsGrounded = Physics2D.OverlapCircle(legs.position, groundRadius, Ground);

        if (IsGrounded==true && Input.GetKeyDown(KeyCode.Space))
        {
            rd.AddForce(transform.up * jump_force, ForceMode2D.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && Mathf.Abs(HorizontalMove) > 0 && (IsGrounded = true))
        {
            StartCoroutine(Dash());
        }

    
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }


        if (!IsGrounded)
        {
            animator.SetBool("Jumping", true);
        }
        else
        {
            animator.SetBool("Jumping", false);
        }

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
        Bar.fillAmount=(float)currentHealth/maxHealth;
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

    public void Die()
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
        if (!isDead && !isDashing)
        {
            Vector2 targetVelocity = new Vector2(HorizontalMove * 10f, rd.velocity.y);
            rd.velocity = targetVelocity;
           
        }
    }

    private void Flip()
    {
        FacingRight = !FacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        
        float originalGravity = rd.gravityScale;
        rd.gravityScale = 0;

        
        float dashDirection = FacingRight ? 1f : -1f;

 
        rd.velocity = new Vector2(dashDirection * dashForce, 0f);

       
        animator.SetTrigger("Dash");

       
        yield return new WaitForSeconds(dashDuration);

      
        rd.gravityScale = originalGravity;
        isDashing = false;
    }
}