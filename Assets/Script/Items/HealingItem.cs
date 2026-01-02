using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingItem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("healing");
            animator.SetTrigger(Animator.StringToHash("Collect"));
            other.GetComponent<PlayerController>().ApplyHeal(5);
            Destroy(gameObject,2f);
        }
    }
}
