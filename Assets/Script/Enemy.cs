using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    
    [SerializeField] private float lifepoint;
    [SerializeField] private float maxLife;
    [Header("Références")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyUI ui;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject bulletPrefab;
    
    [Header("Statistique")]
    [SerializeField] private float attackRange = 10f; 
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private int damage;
    [SerializeField] private LayerMask obstacleMask;
    
    // Variables privées
    private float nextFireTime;
    private float pathUpdateTimer;
    private bool hasLineOfSight = false;
    private bool dead;
    
    void Start()
    {
        // Récupérer les composants
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
            
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Vérifier NavMesh
        if (agent != null && !agent.isOnNavMesh)
        {
            Debug.LogError("L'ennemi " + gameObject.name + " n'est pas sur le NavMesh !");
            enabled = false;
            return;
        }
        dead = false;
    }
    
    void Update()
    {
        if(dead) return;
        if (player == null || agent == null || !agent.isOnNavMesh) return;
        
        pathUpdateTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Vérifier la ligne de vue
        hasLineOfSight = CheckLineOfSight();
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
            animator.SetBool("isMoving", false);
        }
        else
        {
            ChasePlayer();
            animator.SetBool("isMoving", true);
        }
        LookAtTarget(player.position);
    }
    
    void ChasePlayer()
    {
        // Mettre à jour le chemin régulièrement
        if (pathUpdateTimer >= 0.5f)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            pathUpdateTimer = 0f;
        }
    }
    
    void AttackPlayer()
    {
        // Arrêter de bouger
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        // Tirer si ligne de vue dégagée
        if (hasLineOfSight && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }
    
    bool CheckLineOfSight()
    {
        // Point de départ du raycast
        Vector3 startPos = transform.position;
        // Point cible (corps du joueur)
        Vector3 targetPos = player.position + Vector3.up;
        Vector3 direction = targetPos - startPos;
        float distance = direction.magnitude;
        
        RaycastHit hit;
        if (Physics.Raycast(startPos, direction.normalized, out hit, distance, ~0)) // ~0 = tous les layers
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                return true;
            }
            if (obstacleMask == (obstacleMask | (1 << hit.transform.gameObject.layer)))
            {
                return false;
            }
            return true;
        }
        return false;
    }
    
    void Fire()
    {
        Debug.Log("ENNEMI TIRE !");
        var direction = transform.rotation * Vector3.forward;
        Bullet bullet = Instantiate(bulletPrefab, transform.position + direction+Vector3.up, transform.rotation).GetComponent<Bullet>();
        bullet.Initialize(damage, bulletSpeed);
        animator.SetTrigger("Shoot");
    }
    
    void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Garder l'ennemi droit
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }
       
    public void ApplyDamage(int damage)
    {
        lifepoint -= damage;
        ui.UpdateLife(lifepoint, maxLife);
        if (lifepoint <= 0)
        {
            animator.SetTrigger("Dying");
            dead = true;
            Destroy(gameObject, 2f);
        }
    }
}