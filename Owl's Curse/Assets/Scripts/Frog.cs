using UnityEngine;

public class Frog : MonoBehaviour
{
    public Animator animator;
    public int maxHealth = 10;
    private int currentHealth;
    private Collider2D frogCollider;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        frogCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Жаба умерла");
        animator.SetBool("IsDied", true);

    
        gameObject.layer = LayerMask.NameToLayer("DeadEnemies");

        rb.isKinematic = true;

        this.enabled = false;
    }
}