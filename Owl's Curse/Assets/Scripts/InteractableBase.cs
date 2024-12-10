using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected float interactionRadius = 2f;
    [SerializeField] protected string promptMessage = "Нажмите E для взаимодействия";

    public abstract void Interact();

    public string GetInteractionPrompt()
    {
        Debug.Log($"Получение сообщения подсказки: {promptMessage}");
        return promptMessage;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    public bool IsPlayerInRange(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            Debug.LogError("PlayerTransform равен null!");
            return false;
        }

        float distance = Vector2.Distance(playerTransform.position, transform.position);
        Debug.Log($"Проверка расстояния для {gameObject.name}:");
        Debug.Log($"Положение игрока: {playerTransform.position}");
        Debug.Log($"Положение этого объекта: {transform.position}");
        Debug.Log($"Расстояние: {distance}, Радиус: {interactionRadius}");

        bool inRange = distance <= interactionRadius;
        Debug.Log($"В пределах диапазона: {inRange}");
        return inRange;
    }
}