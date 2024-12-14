using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("UI Elements")]
    [SerializeField] private Canvas gameCanvas;
    [SerializeField] private Image imageToShow;
    [SerializeField] private Button buttonToShow;
    [SerializeField] private TextMeshProUGUI textToShow;

    [Header("Target and Animation Settings")]
    [SerializeField] private GameObject targetObject;  
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float elementDelay = 0.3f;

    [SerializeField] private Transform minPoint;
    [SerializeField] private Transform maxPoint;

    private void Start()
    {
        owlAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        owlAnimator.SetBool("IsWalking", false);
        owlAnimator.SetBool("IsFlying", false);
        player = GameObject.FindGameObjectWithTag("Player");

        if (gameCanvas != null)
        {
            imageToShow.gameObject.SetActive(false);
            buttonToShow.gameObject.SetActive(false);
            textToShow.gameObject.SetActive(false);
        }

        buttonToShow.onClick.AddListener(OnButtonClick);
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
        float newPosition = transform.position.x + horizontalMove * moveSpeed * Time.deltaTime;

        if (newPosition < minPoint.position.x)
        {
            newPosition = minPoint.position.x;
        }
        else if (newPosition > maxPoint.position.x)
        {
            newPosition = maxPoint.position.x;
        }

        transform.position = new Vector3(newPosition, transform.position.y, transform.position.z);

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

        if (gameCanvas != null)
        {
            imageToShow.gameObject.SetActive(false);
            buttonToShow.gameObject.SetActive(false);
            textToShow.gameObject.SetActive(false);
        }
    }

    private void FlyToTarget()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);


        if (transform.position == targetPosition)
        {
            isFlyingToTarget = false;
            ShowUIElements();
        }
    }

    private void ShowUIElements()
    {
        if (gameCanvas != null)
        {
            StartCoroutine(ShowElementsWithFadeIn());
        }
    }

    private IEnumerator ShowElementsWithFadeIn()
    {

        if (imageToShow != null)
        {
            imageToShow.gameObject.SetActive(true);
            yield return StartCoroutine(FadeImage(imageToShow, 0f, 1f));
        }

        yield return new WaitForSeconds(elementDelay);


        if (textToShow != null)
        {
            textToShow.gameObject.SetActive(true);
            yield return StartCoroutine(FadeText(textToShow, 0f, 1f));
        }

        yield return new WaitForSeconds(elementDelay);

        if (buttonToShow != null)
        {
            buttonToShow.gameObject.SetActive(true);
            Image buttonImage = buttonToShow.GetComponent<Image>();
            TextMeshProUGUI buttonText = buttonToShow.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonImage != null)
            {
                yield return StartCoroutine(FadeImage(buttonImage, 0f, 1f));
            }

            if (buttonText != null)
            {
                yield return StartCoroutine(FadeText(buttonText, 0f, 1f));
            }
        }
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float targetAlpha)
    {
        Color color = image.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeInDuration);
            image.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float targetAlpha)
    {
        Color color = text.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeInDuration);
            text.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        text.color = color;
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

    private void OnButtonClick()
    {
        Debug.Log("Button clicked!");
    }
}
