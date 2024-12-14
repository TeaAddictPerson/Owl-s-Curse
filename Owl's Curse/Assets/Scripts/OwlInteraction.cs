using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class OwlInteraction : InteractableBase
{
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private string interactionPrompt = "Вы хотите сесть на сову?";
    [SerializeField] private GameObject owlSprite;
    [SerializeField] private GameObject player;
    [SerializeField] private Sprite owlWithPlayerSprite;
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;

    private bool isInteracting = false;
    private Transform playerTransform;
    private PlayerScript playerScript;
    private bool isPlayerOnOwl = false;
    public FrameSwitch frameSwitch;

    private Animator playerAnimator;
    private Animator owlAnimator;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerScript = player.GetComponent<PlayerScript>();

        if (interactionPanel == null || interactionText == null || yesButton == null || noButton == null)
        {
            return;
        }

        if (cinemachineCamera == null)
        {
            return;
        }

        playerAnimator = player.GetComponent<Animator>();
        owlAnimator = owlSprite.GetComponent<Animator>();

        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
        HideUI();
    }

    private void Update()
    {
        if (IsPlayerInRange(playerTransform))
        {
            if (!isInteracting && Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
        else
        {
            if (isInteracting)
            {
                HideUI();
            }
        }
    }

    public override void Interact()
    {
        if (playerScript != null)
        {
            playerScript.IsInputBlocked = true;
            playerScript.enabled = false;
        }

        interactionPanel.SetActive(true);
        interactionText.gameObject.SetActive(true);
        interactionText.text = interactionPrompt;

        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);

        isInteracting = true;
    }

    private void OnYesButtonClicked()
    {
        SpriteRenderer playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.enabled = false;
        }

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        isPlayerOnOwl = true;

        SpriteRenderer owlSpriteRenderer = owlSprite.GetComponent<SpriteRenderer>();
        if (owlSpriteRenderer != null)
        {
            owlSpriteRenderer.sprite = owlWithPlayerSprite;
        }

        owlSprite.tag = "Player";
        owlSprite.GetComponent<OwlController>().SetPlayerOnOwl(true);

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isWalking", false);
        }

        if (owlAnimator != null)
        {
            owlAnimator.SetBool("isWalking", false);
            owlAnimator.SetBool("isRiding", true);
        }

        if (frameSwitch != null)
        {
            frameSwitch.activeFrame.SetActive(true);
        }

        HideUI();
    }

    private void OnNoButtonClicked()
    {
        HideUI();
    }

    private void HideUI()
    {
        if (playerScript != null)
        {
            playerScript.enabled = true;
            playerScript.IsInputBlocked = false;
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }

        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(false);
        }

        if (noButton != null)
        {
            noButton.gameObject.SetActive(false);
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        isInteracting = false;
    }

    public new bool IsPlayerInRange(Transform playerTransform)
    {
        float distance = Vector2.Distance(playerTransform.position, transform.position);
        bool inRange = distance <= interactionRadius;
        return inRange;
    }
}