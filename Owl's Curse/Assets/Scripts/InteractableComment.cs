using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableComment : InteractableBase
{
    [SerializeField] private GameObject commentPanel;
    [SerializeField] private TextMeshProUGUI commentText;
    [TextArea(2, 5)]
    [SerializeField] private string characterComment;

    private Transform player;

    private void Start()
    {
        Debug.Log($"InteractableComment запущен на {gameObject.name}");

        if (commentPanel == null)
        {
            Debug.LogError("Панель комментариев не назначена!");
            return;
        }
        else
        {
            Debug.Log($"Панель комментариев найдена: {commentPanel.name}, активна: {commentPanel.activeSelf}");
            if (commentPanel.transform.parent != null)
                Debug.Log($"Родитель панели: {commentPanel.transform.parent.name}");
        }

        if (commentText == null)
        {
            Debug.LogError("Текст комментария не назначен!");
        }
        else
        {
            Debug.Log($"Компонент текста комментария найден на: {commentText.gameObject.name}");
        }

        if (string.IsNullOrEmpty(characterComment))
            Debug.LogWarning("Комментарий персонажа пуст!");

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Игрок не найден! Убедитесь, что у игрока есть тег 'Player'");
        }

        commentPanel.SetActive(false);
    }

    private void Update()
    {
        if (commentPanel != null && commentPanel.activeSelf && player != null)
        {
            if (!IsPlayerInRange(player))
            {
                commentPanel.SetActive(false);
                if (commentText != null)
                {
                    commentText.enabled = false;
                }
            }
        }
    }

    public override void Interact()
    {
        Debug.Log($"Взаимодействие вызвано на комментарии: {gameObject.name}");
        if (commentPanel == null)
        {
            Debug.LogError($"Панель комментариев равна null на {gameObject.name}!");
            return;
        }

        Debug.Log($"Состояние панели комментариев перед переключением: {commentPanel.activeSelf}, имя панели: {commentPanel.name}");
        commentPanel.SetActive(!commentPanel.activeSelf);
        Debug.Log($"Состояние панели комментариев после переключения: {commentPanel.activeSelf}");

        if (commentPanel.activeSelf)
        {
            if (commentText == null)
            {
                Debug.LogError($"Компонент текста комментария равен null на {gameObject.name}!");
                return;
            }

            Debug.Log($"Установка текста комментария: '{characterComment}' на {commentText.gameObject.name}");
            commentText.text = characterComment;
            commentText.enabled = true;
        }
        else
        {
            if (commentText != null)
            {
                commentText.enabled = false;
            }
        }
    }
}