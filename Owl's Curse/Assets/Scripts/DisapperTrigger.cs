using UnityEngine;

public class DisappearTrigger : MonoBehaviour
{
    public RavenBoss boss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player != null && boss != null)
            {
                boss.OnDisappearTriggerEnter(player);
            }
        }
    }
}