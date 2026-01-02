using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadItem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Reload");
            animator.SetTrigger(Animator.StringToHash("Collect"));
            other.GetComponent<PlayerController>().EnhanceReload();
            Destroy(gameObject,2f);
        }
    }
}
