using UnityEngine;

public class FrameSwitch : MonoBehaviour
{
    public GameObject activeFrame;
    public bool isPlayerOnOwl = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPlayerOnOwl)  
        {
            activeFrame.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPlayerOnOwl) 
        {
            activeFrame.SetActive(false);
        }
    }
}
