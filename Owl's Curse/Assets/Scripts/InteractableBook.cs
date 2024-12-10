using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableBook : InteractableBase
{
    [SerializeField] private GameObject bookPanel;
    [SerializeField] private TextMeshProUGUI bookText;
    [TextArea(3, 10)]
    [SerializeField] private string bookContent;

    private Transform player;

    private void Start()
    {
        Debug.Log($"InteractableBook запущен на {gameObject.name}");

        if (bookPanel == null)
        {
            Debug.LogError("Панель книги не назначена!");
            return;
        }
        else
        {
            Debug.Log($"Панель книги найдена: {bookPanel.name}, активна: {bookPanel.activeSelf}");
            if (bookPanel.transform.parent != null)
                Debug.Log($"Родитель панели: {bookPanel.transform.parent.name}");
        }

        if (bookText == null)
        {
            Debug.LogError("Текст книги не назначен!");
        }
        else
        {
            Debug.Log($"Компонент текста книги найден на: {bookText.gameObject.name}");
        }

        if (string.IsNullOrEmpty(bookContent))
            Debug.LogWarning("Содержимое книги пусто!");

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Игрок не найден! Убедитесь, что у игрока есть тег 'Player'");
        }

        bookPanel.SetActive(false);
    }

    private void Update()
    {
        if (bookPanel != null && bookPanel.activeSelf && player != null)
        {
            if (!IsPlayerInRange(player))
            {
                bookPanel.SetActive(false);
                if (bookText != null)
                {
                    bookText.enabled = false;
                }
            }
        }
    }

    public override void Interact()
    {
        Debug.Log($"Взаимодействие вызвано на книге: {gameObject.name}");
        if (bookPanel == null)
        {
            Debug.LogError($"Панель книги равна null на {gameObject.name}!");
            return;
        }

        Debug.Log($"Состояние панели книги перед переключением: {bookPanel.activeSelf}, имя панели: {bookPanel.name}");
        bookPanel.SetActive(!bookPanel.activeSelf);
        Debug.Log($"Состояние панели книги после переключения: {bookPanel.activeSelf}");

        if (bookPanel.activeSelf)
        {
            if (bookText == null)
            {
                Debug.LogError($"Компонент текста книги равен null на {gameObject.name}!");
                return;
            }

            Debug.Log($"Установка текста книги: '{bookContent}' на {bookText.gameObject.name}");
            bookText.text = bookContent;
            bookText.enabled = true;
        }
        else
        {
            if (bookText != null)
            {
                bookText.enabled = false;
            }
        }
    }
}