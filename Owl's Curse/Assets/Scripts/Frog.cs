using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : MonoBehaviour
{
    public Animator animator;
    public int maxHealth=10;
    int currentHealth;
    void Start()
    {
        currentHealth=maxHealth;
    }

   
    public void TakeDamage(int damage)
    {
        currentHealth-=damage;
        animator.SetTrigger("Hurt");

        if(currentHealth<=0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Жаба умерла");
        animator.SetBool("IsDied", true);
        GetComponent<Collider2D>().enabled=false;
        this.enabled=false;
    }
}
