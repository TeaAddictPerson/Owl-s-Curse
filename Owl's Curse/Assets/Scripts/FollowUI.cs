using UnityEngine;

public class FollowUI : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0); 

    private void LateUpdate()
    {
        if (player != null)
        {
           
            transform.position = player.position + offset;

           
            transform.forward = Camera.main.transform.forward;
        }
    }
}