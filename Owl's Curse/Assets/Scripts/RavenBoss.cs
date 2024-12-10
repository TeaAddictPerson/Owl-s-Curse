using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RavenBoss : MonoBehaviour, IDamageable, ISanityDamage
{
    [Header("Основные параметры")]
    public int maxHealth = 1000;
    private int currentHealth;
    public Image healthBar;
    public float detectionRange = 10f;
    private bool isAwakened = false;
    private Animator animator;
    private Transform player;
    private Rigidbody2D rb;

    [Header("Атаки")]
    public float attackRange = 3f; 
    public float pushAttackRange = 8f; 
    public float moveSpeed = 3f;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("Точки атаки")]
    public Transform beakAttackPoint;
    public Transform clawAttackPoint;
    public Transform wingsAttackPoint;

    [Header("Урон")]
    public int beakDamage = 20;
    public int clawDamage = 15;
    public float pushForce = 15f;

    [Header("Урон рассудку")]
    public float sanityDamage = 0.5f;

    private Vector3 originalScale;
    private bool facingRight = true;
    public float GetSanityDamage()
    {
        return sanityDamage;
    }

    private bool isDead = false; 

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        originalScale = transform.localScale;
        UpdateHealthBar();
    }

    private bool isMoving = false;

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

      
        if (!isAwakened && distanceToPlayer <= detectionRange)
        {
            StartCoroutine(AwakeningSequence());
            return;
        }

        if (!isAwakened) return;

       
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (distanceToPlayer <= attackRange)
            {
                isMoving = false;
                animator.SetBool("IsWalking", false);
               
                if (Random.value > 0.5f)
                    StartCoroutine(BeakAttack());
                else
                    StartCoroutine(ClawAttack());
            }
            else if (distanceToPlayer <= pushAttackRange)
            {
                isMoving = false;
                animator.SetBool("IsWalking", false);
                StartCoroutine(WingsPushAttack());
            }
            else
            {
            
                isMoving = true;
                animator.SetBool("IsWalking", true);
                MoveTowardsPlayer();
            }
        }
    }

    private void FlipBoss(bool toRight)
    {
        if (facingRight != toRight)
        {
            facingRight = toRight;
           
            transform.localScale = new Vector3(
                originalScale.x * (facingRight ? 1 : -1),
                originalScale.y,
                originalScale.z
            );
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

      
        FlipBoss(direction.x < 0);
    }

    private IEnumerator BeakAttack()
    {
        lastAttackTime = Time.time;
        rb.velocity = Vector2.zero;

       
        FlipBoss((player.position.x - transform.position.x) < 0);

        animator.SetTrigger("BeakAttack");
        yield return new WaitForSeconds(0.5f);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(beakAttackPoint.position, 1f);
        foreach (Collider2D player in hitPlayers)
        {
            PlayerScript playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(beakDamage, false, this);
            }
        }
    }

    private IEnumerator ClawAttack()
    {
        lastAttackTime = Time.time;
        rb.velocity = Vector2.zero;

      
        FlipBoss((player.position.x - transform.position.x) < 0);

        animator.SetTrigger("ClawAttack");
        yield return new WaitForSeconds(0.5f);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(clawAttackPoint.position, 1.5f);
        foreach (Collider2D player in hitPlayers)
        {
            PlayerScript playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(clawDamage, false, this);
            }
        }
    }

    private IEnumerator WingsPushAttack()
    {
        lastAttackTime = Time.time;
        rb.velocity = Vector2.zero;

    
        FlipBoss((player.position.x - transform.position.x) < 0);

        animator.SetTrigger("WingsAttack");
        yield return new WaitForSeconds(0.5f);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(wingsAttackPoint.position, 2f);
        foreach (Collider2D player in hitPlayers)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 pushDirection = (player.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
            }
        }
    }

    private IEnumerator AwakeningSequence()
    {
        isAwakened = true;
        
        transform.localScale = originalScale;
        animator.SetTrigger("Awaken");
        yield return new WaitForSeconds(2f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        
        transform.localScale = new Vector3(
            originalScale.x * (facingRight ? 1 : -1),
            originalScale.y,
            originalScale.z
        );

        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
       
        transform.localScale = new Vector3(
            originalScale.x * (facingRight ? 1 : -1),
            originalScale.y,
            originalScale.z
        );
        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        rb.isKinematic = true;
        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
     
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pushAttackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

   
        if (beakAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(beakAttackPoint.position, 1f);
        }
        if (clawAttackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(clawAttackPoint.position, 1.5f);
        }
        if (wingsAttackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wingsAttackPoint.position, 2f);
        }
    }
}