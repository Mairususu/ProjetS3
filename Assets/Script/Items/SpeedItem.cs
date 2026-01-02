using UnityEngine;

public class SpeedItem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Speed");
            animator.SetTrigger(Animator.StringToHash("Collect"));
            other.GetComponent<PlayerController>().EnhanceSpeed();
            Destroy(gameObject,2f);
        }
    }
}
