﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

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

    [Header("Взаимодействие")]
    private InteractableBase currentInteractable;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Рассудок")]
    public float maxSanity = 5f;
    public float currentSanity;
    public float sanityLossOnDamage = 3f;
    public float healthLossRate = 1f; 
    private bool isInsane = false;
    public Image sanityBar;

    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentSanity = maxSanity;
    }

    private void Update()
    {
        if (isDead) return;

        
        IsGrounded = Physics2D.OverlapCircle(legs.position, groundRadius, Ground);

        if (!isInsane)
        {
           
            if (IsGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                rd.AddForce(transform.up * jump_force, ForceMode2D.Impulse);
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && Mathf.Abs(HorizontalMove) > 0 && IsGrounded)
            {
                StartCoroutine(Dash());
            }

            HorizontalMove = Input.GetAxisRaw("Horizontal") * speed;
        }

     
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }

    
        animator.SetBool("Jumping", !IsGrounded);
        animator.SetFloat("HorizontalMove", Mathf.Abs(HorizontalMove));

       
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            }
        }

   
        if (!isInsane && Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

       
        if (HorizontalMove < 0 && FacingRight)
        {
            Flip();
        }
        else if (HorizontalMove > 0 && !FacingRight)
        {
            Flip();
        }

        HandleInteraction();
    }

    private IEnumerator InsanityBehavior()
    {
        isInsane = true;

        while (currentSanity <= 0 && !isDead)
        {
         
            float randomX = Random.Range(-1f, 1f);
            HorizontalMove = randomX * speed;

        
            rd.velocity = new Vector2(randomX * speed * 10f, rd.velocity.y);

            
            if (IsGrounded && Random.value > 0.95f)
            {
                rd.AddForce(transform.up * jump_force, ForceMode2D.Impulse);
                animator.SetBool("Jumping", true);
            }

           
            if (randomX < 0 && FacingRight)
            {
                Flip();
            }
            else if (randomX > 0 && !FacingRight)
            {
                Flip();
            }

          
            animator.SetFloat("HorizontalMove", Mathf.Abs(randomX * speed));

          
            TakeDamage(1, true);

            yield return new WaitForSeconds(healthLossRate);
        }

        isInsane = false;
    }

    public void TakeDamage(int damage, bool fromSanityLoss = false, ISanityDamage sanityDamageSource = null)
    {
        Debug.Log($"Получен урон: {damage}. текущее хп: {currentHealth}");

        if ((isInvincible && !fromSanityLoss) || isDead)
        {
            Debug.Log("Дамаг не прошел игрок умер или в инвизе");
            return;
        }

        currentHealth -= damage;
        Bar.fillAmount = (float)currentHealth / maxHealth;

        if (!fromSanityLoss)
        {
            animator.SetTrigger("Hurt");
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);

            float sanityDamageAmount = sanityDamageSource != null ?
                sanityDamageSource.GetSanityDamage() :
                sanityLossOnDamage;

            currentSanity = Mathf.Max(0, currentSanity - sanityDamageAmount);
            if (sanityBar != null)
                sanityBar.fillAmount = currentSanity / maxSanity;

            if (currentSanity <= 0 && !isInsane)
            {
                StartCoroutine(InsanityBehavior());
            }
        }

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
            if (!isInsane)
            {
              
                Vector2 targetVelocity = new Vector2(HorizontalMove * 10f, rd.velocity.y);
                rd.velocity = targetVelocity;
            }        
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

    private void HandleInteraction()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2f);

        InteractableBase closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (var collider in colliders)
        {
            var interactable = collider.GetComponent<InteractableBase>();
            if (interactable != null && interactable.IsPlayerInRange(transform))
            {
                float distance = Vector2.Distance(transform.position, interactable.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        currentInteractable = closestInteractable;

        if (promptText != null)
        {
            if (currentInteractable != null)
            {
                promptText.text = currentInteractable.GetInteractionPrompt();
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }

    public void RestoreSanity(float amount)
    {
        currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
        if (sanityBar != null)
            sanityBar.fillAmount = currentSanity / maxSanity;
    }
}