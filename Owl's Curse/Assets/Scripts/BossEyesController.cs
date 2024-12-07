using UnityEngine;
using System.Collections;

public class BossEyesController : MonoBehaviour
{
    [Header("Настройки глаз")]
    public float eyesOpenDuration = 3f;
    public float eyesClosedDuration = 2f;
    private bool eyesOpen = false;

    [Header("Настройки атаки")]
    public float attackSpeed = 5f;
    public float detectionRadius = 10f;
    public LayerMask treeLayer;
    public LayerMask playerLayer;
    private bool isAttacking = false;

    [Header("Компоненты")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite angrySprite;
    public Sprite eyesClosedSprite;
    private Animator animator;

    private Transform player;
    private Vector3 startPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(EyesCycle());
    }

    private void CheckPlayer()
    {
        if (!eyesOpen || isAttacking) return;

       
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider != null)
        {
            Vector2 directionToPlayer = (playerCollider.transform.position - transform.position).normalized;

          
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position,
                directionToPlayer,
                detectionRadius
            );

          
            System.Array.Sort(hits, (a, b) =>
                a.distance.CompareTo(b.distance));

            bool playerFound = false;
            bool treeFound = false;

          
            foreach (RaycastHit2D hit in hits)
            {
          
                if (((1 << hit.collider.gameObject.layer) & treeLayer) != 0)
                {
                    treeFound = true;
                    break;
                }

              
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    playerFound = true;
                    break;
                }
            }

            if (playerFound && !treeFound)
            {
                StartCoroutine(AttackPlayer());
            }
        }
    }

    private IEnumerator AttackPlayer()
    {
        if (isAttacking) yield break; 

        isAttacking = true;

       
        spriteRenderer.sprite = angrySprite;

       
        animator.SetTrigger("Angry");

        Debug.Log("Босс атакует"); 

       
        yield return new WaitForSeconds(0.5f);

        float elapsedTime = 0;
        Vector3 startPos = transform.position;

        
        while (elapsedTime < 1.0f && Vector2.Distance(transform.position, player.position) > 0.1f)
        {
            elapsedTime += Time.deltaTime * attackSpeed;
            transform.position = Vector3.Lerp(startPos, player.position, elapsedTime);
            yield return null;
        }

        if (Vector2.Distance(transform.position, player.position) <= 0.5f)
        {
            PlayerScript playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                Debug.Log("убиваем гг"); 
                playerScript.Die();
            }
        }

     
        yield return new WaitForSeconds(1f);
        transform.position = startPosition;

      
        spriteRenderer.sprite = normalSprite;

        isAttacking = false;

     
        StartCoroutine(EyesCycle());
    }

    private IEnumerator EyesCycle()
    {
        while (!isAttacking)
        {
           
            eyesOpen = true;
            spriteRenderer.sprite = normalSprite;
            animator.SetTrigger("OpenEyes");

       
            float checkTimer = eyesOpenDuration;
            while (checkTimer > 0 && !isAttacking)
            {
                CheckPlayer();
                checkTimer -= Time.deltaTime;
                yield return null;
            }

            if (!isAttacking)
            {
              
                eyesOpen = false;
                spriteRenderer.sprite = eyesClosedSprite;
                animator.SetTrigger("CloseEyes");
                yield return new WaitForSeconds(eyesClosedDuration);
            }
        }
    }

    void OnDrawGizmos()
    {
      
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

    
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Vector2 direction = (player.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * detectionRadius);
        }
    }
}