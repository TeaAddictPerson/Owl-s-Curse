using UnityEngine;

public class SanityPickup : MonoBehaviour
{
    public float sanityRestoreAmount = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.RestoreSanity(sanityRestoreAmount);
                Destroy(gameObject);
            }
        }
    }
}