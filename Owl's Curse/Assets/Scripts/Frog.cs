using UnityEngine;

public class Frog : MonoBehaviour, IDamageable
{
    public Animator animator;
    public int maxHealth = 10;
    private int currentHealth;
    private Collider2D frogCollider;
    private Rigidbody2D rb;

    [Header("Преследование")]
    public float moveSpeed = 3f;
    public float jumpForce = 3f;
    public float detectionRange = 5f; 
    public float maxChaseDistance = 8f; 
    public float attackRange = 1f; 
    public float attackCooldown = 2f; 
    private float lastAttackTime;

    private Transform player;

    void Start()
    {
        currentHealth = maxHealth;
        frogCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

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
    }

    void Update()
    {
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
            playerScript.TakeDamage(1);
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

    void Die()
    {
        Debug.Log("Жаба умерла");
        animator.SetBool("IsDied", true);
        gameObject.layer = LayerMask.NameToLayer("DeadEnemies");

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}