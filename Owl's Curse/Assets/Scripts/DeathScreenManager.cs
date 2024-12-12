using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image deathScreenImage;
    [SerializeField] private Button respawnButton;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private PlayerSpawner playerSpawner;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float elementDelay = 0.3f;

    private void Start()
    {
        if (deathScreenImage == null || respawnButton == null || deathText == null)
        {
            Debug.LogError("Не назначены UI компоненты!");
            return;
        }

        // Скрываем все элементы при старте
        deathScreenImage.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);

        respawnButton.onClick.AddListener(RespawnPlayer);

        if (playerSpawner == null)
        {
            playerSpawner = FindObjectOfType<PlayerSpawner>();
        }
    }

    public void ShowDeathScreen()
    {
        Debug.Log("Показываем экран смерти");
        StartCoroutine(ShowDeathScreenElements());
    }

    private IEnumerator ShowDeathScreenElements()
    {
        // Показываем и анимируем фоновое изображение
        deathScreenImage.gameObject.SetActive(true);
        yield return StartCoroutine(FadeImage(deathScreenImage, 0, 1));

        // Показываем текст
        yield return new WaitForSeconds(elementDelay);
        deathText.gameObject.SetActive(true);
        yield return StartCoroutine(FadeText(deathText, 0, 1));

        // Показываем кнопку
        yield return new WaitForSeconds(elementDelay);
        respawnButton.gameObject.SetActive(true);
        Image buttonImage = respawnButton.GetComponent<Image>();
        TextMeshProUGUI buttonText = respawnButton.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonImage != null)
        {
            yield return StartCoroutine(FadeImage(buttonImage, 0, 1));
        }
        if (buttonText != null)
        {
            yield return StartCoroutine(FadeText(buttonText, 0, 1));
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

    private void RespawnPlayer()
    {
        Debug.Log("Начинаем возрождение игрока");

        string saveData = PlayerPrefs.GetString("LastSave");
        if (string.IsNullOrEmpty(saveData))
        {
            Debug.LogError("Нет данных о последнем сохранении!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Игрок не найден!");
            return;
        }

        PlayerScript playerScript = player.GetComponent<PlayerScript>();
        if (playerScript == null)
        {
            Debug.LogError("Компонент PlayerScript не найден!");
            return;
        }

        string[] saveParams = saveData.Split(',');
        if (saveParams.Length >= 4)
        {
            playerScript.maxHealth = int.Parse(saveParams[1]);
            playerScript.currentHealth = playerScript.maxHealth;
            playerScript.attackDamage = int.Parse(saveParams[2]);
            playerScript.currentSanity = float.Parse(saveParams[3]);

            if (playerScript.Bar != null)
            {
                playerScript.Bar.fillAmount = 1f;
            }
            if (playerScript.sanityBar != null)
            {
                playerScript.sanityBar.fillAmount = playerScript.currentSanity / playerScript.maxSanity;
            }

            playerScript.isDead = false;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            if (playerScript.animator != null)
            {
                playerScript.animator.SetBool("IsDead", false);
            }

            Debug.Log($"Характеристики восстановлены: HP={playerScript.maxHealth}, DMG={playerScript.attackDamage}, Sanity={playerScript.currentSanity}");
        }

        if (playerSpawner != null)
        {
            playerSpawner.RespawnPlayerAtLastSave();
        }

        // Скрываем все элементы
        deathScreenImage.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);

        // Сбрасываем прозрачность всех элементов
        ResetElementsAlpha();

        Debug.Log("Игрок успешно возрожден");
    }

    private void ResetElementsAlpha()
    {
        Color color;

        // Сброс прозрачности изображения
        color = deathScreenImage.color;
        color.a = 1f;
        deathScreenImage.color = color;

        // Сброс прозрачности текста
        color = deathText.color;
        color.a = 1f;
        deathText.color = color;

        // Сброс прозрачности кнопки
        Image buttonImage = respawnButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            color = buttonImage.color;
            color.a = 1f;
            buttonImage.color = color;
        }

        TextMeshProUGUI buttonText = respawnButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            color = buttonText.color;
            color.a = 1f;
            buttonText.color = color;
        }
    }

    private void OnDestroy()
    {
        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveListener(RespawnPlayer);
        }
    }
}