using UnityEngine;

public class BossAttackPoint : MonoBehaviour
{
    public int damage;
    public float radius;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}