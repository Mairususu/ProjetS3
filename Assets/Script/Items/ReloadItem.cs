using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadItem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("healing");
            animator.SetTrigger(Animator.StringToHash("IsCollected"));
            other.GetComponent<PlayerController>().ApplyHeal(1);
            Destroy(gameObject,2f);
        }
    }
}
