using UnityEngine;
using System.Collections;

public class FinalRoomTrigger : MonoBehaviour
{
    public Collider2D leftWallCollider;
    public Collider2D rightWallCollider;

    private bool isTriggered = false;

    private void Start()
    {
        if (leftWallCollider == null || rightWallCollider == null)
        {
            Debug.LogError("Один или оба коллайдера стен не установлены!");
            return;
        }

        leftWallCollider.isTrigger = true;
        rightWallCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            Debug.Log("Игрок вошел в комнату. Ждем 2 секунды перед отключением триггеров.");

            isTriggered = true;
            StartCoroutine(DisableTriggersAfterDelay(2f));
        }
    }

    private IEnumerator DisableTriggersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        leftWallCollider.isTrigger = false;
        rightWallCollider.isTrigger = false;

        Debug.Log("Триггеры отключены после задержки.");
    }

}
