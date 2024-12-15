using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RavenBoss : MonoBehaviour, IDamageable, ISanityDamage
{
    [Header("Основные параметры")]
    public GameObject healthBarContainer;
    public int maxHealth = 1000;
    public int currentHealth;
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
    public float wingAttackCooldown = 10f;
    private float lastAttackTime;
    private float lastWingAttackTime;

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

    [Header("Физика")]
    public bool useGravity = true;
    public float gravityScale = 1f;

    [Header("Проверка расстояния")]
    public Vector2 detectionBoxSize = new Vector2(4f, 3f);
    public Vector2 attackBoxSize = new Vector2(3f, 2f);

    [Header("Frame Settings")]
    public Collider2D frameCollider;

    [Header("Настройки смерти")]
    public Collider2D disappearTrigger;

    private PlayerScript playerScript;
    private bool isPlayerDead = false;
    private Vector3 originalScale;
    private bool facingRight = true;
    private SpriteRenderer spriteRenderer;

    public Collider2D bodyCollider;
    public Collider2D feetCollider;
    private Vector3 currentPosition;
    private Vector3 initialPosition;
    private float currentY;
    private bool isAwakening = false;


    private bool IsPlayerInRange(Vector2 boxSize, out float distance)
    {
        Vector2 checkPosition = (Vector2)transform.position + new Vector2(0, 1f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(checkPosition, boxSize, 0f);
        distance = Vector2.Distance(transform.position, player.position);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

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
        if (disappearTrigger != null)
        {
            DisappearTrigger triggerComponent = disappearTrigger.gameObject.GetComponent<DisappearTrigger>();
            if (triggerComponent == null)
            {
                triggerComponent = disappearTrigger.gameObject.AddComponent<DisappearTrigger>();
            }
            triggerComponent.boss = this;
        }
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        initialPosition = transform.position;
        currentPosition = initialPosition;
        currentY = initialPosition.y;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerScript>();
        }

        animator.Play("RavenBossAwaken", 0, 0f);
        animator.speed = 0;

        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }

        UpdateHealthBar();
    }

    private bool isMoving = false;

    private void Update()
    {
        if (player == null) return;

        if (playerScript != null && playerScript.isDead && !isPlayerDead)
        {
            isPlayerDead = true;
            OnPlayerDeath();
            return;
        }

        if (isPlayerDead) return;

        float distanceToPlayer;
        bool playerInDetectionRange = IsPlayerInRange(detectionBoxSize, out distanceToPlayer);

        if (!isAwakened && !isAwakening && playerInDetectionRange)
        {
            StartCoroutine(AwakeningSequence());
            return;
        }

        if (!isAwakened)
        {
            transform.position = initialPosition;
            return;
        }

        if (isAwakened && !isAwakening)
        {
            bool playerInAttackRange = IsPlayerInRange(attackBoxSize, out distanceToPlayer);
            Debug.Log($"Игрок в атакующем радиусе: {playerInAttackRange}, Расстояние: {distanceToPlayer}");

            if (playerInAttackRange)
            {
                isMoving = false;
                animator.SetBool("IsWalking", false);

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    if (Time.time >= lastWingAttackTime + wingAttackCooldown)
                    {
                        StartCoroutine(WingsPushAttack());
                    }
                    else
                    {
                        StartCoroutine(Random.value > 0.5f ? BeakAttack() : ClawAttack());
                    }
                }
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

    private IEnumerator AwakeningSequence()
    {
        if (isAwakening) yield break;

        Debug.Log("Начинается последовательность пробуждения");
        isAwakening = true;

        animator.Rebind();
        animator.Update(0f);

        animator.speed = 1;
        animator.SetTrigger("Awaken");

        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        Debug.Log("Пробуждение завершено, начинается движение");

        isAwakened = true;
        isAwakening = false;
        rb.simulated = true;

        isMoving = true;
        animator.SetBool("IsWalking", true);
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 newPosition = rb.position + direction * moveSpeed * Time.deltaTime;
        newPosition.y = currentY;

        Debug.Log($"Движение к позиции: {newPosition}, Текущая позиция: {rb.position}");

        rb.MovePosition(newPosition);
        FlipBoss(direction.x < 0);
    }

    private IEnumerator BeakAttack()
    {
        Debug.Log("Начинается атака клювом");
        lastAttackTime = Time.time;
        isMoving = false;
        animator.SetBool("IsWalking", false);

        Vector2 currentPos = rb.position;
        currentPos.y = currentY;
        rb.MovePosition(currentPos);

        FlipBoss((player.position.x - transform.position.x) < 0);
        animator.SetTrigger("BeakAttack");

        yield return new WaitForSeconds(0.5f);

        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(
            beakAttackPoint.position,
            new Vector2(1f, 2f),
            0f
        );

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
        Vector2 currentPos = rb.position;
        currentPos.y = currentY;
        rb.MovePosition(currentPos);

        FlipBoss((player.position.x - transform.position.x) < 0);
        animator.SetTrigger("ClawAttack");
        yield return new WaitForSeconds(0.5f);
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(clawAttackPoint.position, new Vector2(1.5f, 2f), 0f);
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
        lastWingAttackTime = Time.time;
        lastAttackTime = Time.time;
        Vector2 currentPos = rb.position;
        currentPos.y = currentY;
        rb.MovePosition(currentPos);

        FlipBoss((player.position.x - transform.position.x) < 0);
        animator.SetTrigger("WingsAttack");
        yield return new WaitForSeconds(0.5f);

        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(wingsAttackPoint.position, new Vector2(2f, 2.5f), 0f);
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
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false);
        }

        transform.localScale = new Vector3(
            originalScale.x * (facingRight ? 1 : -1),
            originalScale.y,
            originalScale.z
        );

        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        gameObject.layer = LayerMask.NameToLayer("DeadEnemies");
        rb.isKinematic = true;
        this.enabled = false;

        PlayerScript playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerScript>();
        if (playerScript != null)
        {
            playerScript.IncrementKillCount();
        };

        Debug.Log("Босс убит. Счетчик врагов увеличен.");
    }



    private void OnPlayerDeath()
    {
        StopAllCoroutines();

        animator.ResetTrigger("Awaken");
        animator.ResetTrigger("BeakAttack");
        animator.ResetTrigger("ClawAttack");
        animator.ResetTrigger("WingsAttack");

        isMoving = false;
        animator.SetBool("IsWalking", false);

        animator.SetTrigger("Idle");

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        Debug.Log("Игрок мертв, босс возвращается в состояние ожидания");
    }

    public void OnDisappearTriggerEnter(PlayerScript player)
    {
        if (isDead)
        {
            Debug.Log($"Игрок прошел через триггер, ворон {gameObject.name} исчезает");
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            if (animator != null)
                animator.enabled = false;
        }
    }

    public void ResetBossState()
    {
        isPlayerDead = false;
    }
    private void OnBecameVisible()
    {
        if (!isDead && spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            if (animator != null)
                animator.enabled = true;
        }
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
            Gizmos.DrawWireCube(beakAttackPoint.position, new Vector3(1f, 2f, 0f));
        }
        if (clawAttackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(clawAttackPoint.position, new Vector3(1.5f, 2f, 0f));
        }
        if (wingsAttackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(wingsAttackPoint.position, new Vector3(2f, 2.5f, 0f));
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector2 detectionCenter = (Vector2)transform.position + new Vector2(0, 1f);
        Gizmos.DrawWireCube(detectionCenter, detectionBoxSize);
        Gizmos.DrawWireCube(detectionCenter, attackBoxSize);
    }
}