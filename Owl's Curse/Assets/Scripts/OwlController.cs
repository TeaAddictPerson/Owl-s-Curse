using UnityEngine;
using UnityEngine.UI;

public class OwlController : MonoBehaviour
{
    private Animator owlAnimator;
    private bool isFlying = false;
    private bool isWalking = false;
    private bool isFacingRight = false;
    private float moveSpeed = 3f;
    private bool isPlayerOnOwl = false;
    private GameObject player;
    private bool isGameEnded = false;

    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private bool isFlyingToTarget = false;

    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private GameObject imageToShow;
    [SerializeField] private GameObject targetObject;

    private void Start()
    {
        owlAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        owlAnimator.SetBool("IsWalking", false);
        owlAnimator.SetBool("IsFlying", false);
        player = GameObject.FindGameObjectWithTag("Player");

        if (gameCanvas != null && imageToShow != null)
        {
            imageToShow.SetActive(false);
        }
    }

    private void Update()
    {
        if (isPlayerOnOwl && !isGameEnded)
        {
            HandleOwlMovement();
        }

        if (isFlyingToTarget)
        {
            FlyToTarget();
        }
    }

    public void SetPlayerOnOwl(bool onOwl)
    {
        isPlayerOnOwl = onOwl;

        if (onOwl)
        {
            player.SetActive(false);
        }
        else
        {
            player.SetActive(true);
        }

        if (!onOwl)
        {
            StopWalking();
            StopFlying();
        }
    }

    private void HandleOwlMovement()
    {
        if (!isFlying)
        {
            float horizontalMove = Input.GetAxis("Horizontal");

            if (horizontalMove != 0)
            {
                isWalking = true;
                owlAnimator.SetBool("IsWalking", true);
                MoveOwl(horizontalMove);
            }
            else
            {
                isWalking = false;
                owlAnimator.SetBool("IsWalking", false);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (targetObject != null)
                {
                    StartFlyingToTarget(targetObject.transform.position);
                }
            }
        }
    }

    private void MoveOwl(float horizontalMove)
    {
        transform.Translate(Vector2.right * horizontalMove * moveSpeed * Time.deltaTime);

        if (horizontalMove > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontalMove < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void StartFlyingToTarget(Vector3 target)
    {
        if (isFlying) return;

        isFlying = true;
        isFlyingToTarget = true;
        targetPosition = target;

        owlAnimator.SetBool("IsFlying", true);
        owlAnimator.SetBool("IsWalking", false);

        if (gameCanvas != null && imageToShow != null)
        {
            imageToShow.SetActive(false);
        }
    }

    private void FlyToTarget()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (transform.position == targetPosition)
        {
            isFlyingToTarget = false;
            ShowImage();
        }
    }

    private void ShowImage()
    {
        if (gameCanvas != null && imageToShow != null)
        {
            imageToShow.SetActive(true);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void StopFlying()
    {
        isFlying = false;
        owlAnimator.SetBool("IsFlying", false);
    }

    public void StopWalking()
    {
        isWalking = false;
        owlAnimator.SetBool("IsWalking", false);
    }
}