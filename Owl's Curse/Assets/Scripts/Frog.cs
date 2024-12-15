using UnityEngine;

public class Frog : MonoBehaviour, IDamageable, ISanityDamage
{
    public Animator animator;
    public int maxHealth = 10;
    private int currentHealth;
    private Collider2D frogCollider;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    [Header("Преследование")]
    public float moveSpeed = 3f;
    public float jumpForce = 3f;
    public float detectionRange = 5f;
    public float maxChaseDistance = 8f; 
    public float attackRange = 1f;
    public float attackCooldown = 2f; 
    private float lastAttackTime;

    [Header("Урон рассудку")]
    public float sanityDamage = 0.2f;

    private Transform player;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        frogCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure player has 'Player' tag");
        }

        lastAttackTime = -attackCooldown;

        if (isDead)
        {
            animator.SetBool("IsDied", true);
            gameObject.layer = LayerMask.NameToLayer("DeadEnemies");
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            if (frogCollider != null)
                frogCollider.enabled = false;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (currentHealth <= 0 || player == null || animator == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        var playerScript = player.GetComponent<PlayerScript>();
        if (playerScript != null && playerScript.isDead)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
            return;
        }

        if (distanceToPlayer < detectionRange)
        {
            animator.SetBool("IsRunning", true);
            ChasePlayer();

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
        }
    }

    void ChasePlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        var playerScript = player.GetComponent<PlayerScript>();

        Debug.Log($"Attempting to attack player. Distance: {Vector2.Distance(transform.position, player.position)}");

        Debug.DrawLine(transform.position, player.position, Color.red, 0.5f);

        if (playerScript != null)
        {
            Debug.Log("Player found, attempting to deal damage");
            playerScript.TakeDamage(1, false, this); 
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogError("PlayerScript component not found on player object!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetSanityDamage()
    {
        return sanityDamage;
    }

    void Die()
    {
        Debug.Log("Жаба умерла");
        isDead = true;
        animator.SetBool("IsDied", true);
        gameObject.layer = LayerMask.NameToLayer("DeadEnemies");

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        PlayerScript playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerScript>();
        if (playerScript != null)
        {
            playerScript.IncrementKillCount();
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnBecameInvisible()
    {
        if (isDead && spriteRenderer != null)
        {
            Debug.Log($"Мертвая жаба {gameObject.name} стала невидимой");
            spriteRenderer.enabled = false;

            if (animator != null)
                animator.enabled = false;
        }
    }

    void OnBecameVisible()
    {
        if (isDead && spriteRenderer != null)
        {
            Debug.Log($"Мертвая жаба {gameObject.name} стала видимой");
            spriteRenderer.enabled = true;

            if (animator != null)
                animator.enabled = true;
        }
    }
}