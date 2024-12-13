using UnityEngine;
using System.Collections;

public class BossRoomTrigger : MonoBehaviour
{
    public Collider2D leftWallCollider; 
    public Collider2D rightWallCollider;
    public RavenBoss boss;

    private bool isTriggered = false;

    private void Start()
    {
        if (leftWallCollider == null || rightWallCollider == null)
        {
            Debug.LogError("Один или оба коллайдера стен не установлены!");
            return;
        }

        if (boss == null)
        {
            Debug.LogError("Ссылка на босса не установлена!");
            return;
        }

    
        leftWallCollider.isTrigger = true;
        rightWallCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTriggered && other.CompareTag("Player")) 
        {
            Debug.Log("Игрок вошел в комнату босса. Ждем 3 секунды перед отключением триггеров.");

            isTriggered = true;
            StartCoroutine(DisableTriggersAfterDelay(3f)); 
        }
    }

    private IEnumerator DisableTriggersAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 

        leftWallCollider.isTrigger = false; 
        rightWallCollider.isTrigger = false;

        Debug.Log("Триггеры отключены после задержки.");
    }

    private void Update()
    {
        if (isTriggered && boss != null && boss.currentHealth <= 0) 
        {
            Debug.Log("Босс побежден. Коллайдеры снова становятся триггерами.");

        
            leftWallCollider.isTrigger = true;
            rightWallCollider.isTrigger = true;

            isTriggered = false;
        }
    }
}
