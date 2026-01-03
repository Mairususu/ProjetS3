using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Références")]
    [SerializeField] private CameraManager cameraPosition; // Nouveau script caméra
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Shooter playerShooter;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private PlayerUI playerUI;
    
    [Header("Déplacement")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Rotation du personnage")]
    [SerializeField] private float playerRotationSpeed = 10f;
    [SerializeField] private bool rotateWithCamera = true;
    
    // Variables privées - Déplacement
    private Vector3 velocity;
    private bool jumpEnable = true;
    private bool shootEnable = true;
    private Vector3 moveDirection;
    
    void Start()
    {
        // Récupérer CameraPosition si non assigné
        if (cameraPosition == null)
        {
            cameraPosition = FindObjectOfType<CameraManager>();
            if (cameraPosition != null)
            {
                cameraPosition.SetTarget(transform);
            }
        }
        
        InitUI();
    }
    
    void Update()
    {
        HandleMovement();
    }
    
    void LateUpdate()
    {
        RotatePlayer();
    }
    
    #region Movement
    
    void HandleMovement()
    {
        if (characterController == null) return;
        
        // Récupérer les inputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Obtenir les directions de la caméra
        Vector3 forward = cameraPosition.GetForwardDirection();
        Vector3 right = cameraPosition.GetRightDirection();
        
        // Direction de déplacement
        moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Vitesse (marche ou course)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? 
            playerShooter.speed + playerShooter.sprint : playerShooter.speed;
        
        // Appliquer le déplacement horizontal
        Vector3 move = moveDirection * currentSpeed;
        characterController.Move(move * Time.deltaTime);
        
        // Saut
        if (Input.GetButtonDown("Jump") && jumpEnable)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            StartCoroutine(JumpCorr());
        }
        
        // Tir
        if (Input.GetMouseButtonDown(1) && shootEnable)
        {
            Shoot();
            StartCoroutine(ShootCorr());
        }
        
        // Appliquer la gravité
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        
        // Animation
        animator.SetBool("isMoving", horizontal != 0f || vertical != 0f);
    }
    
    IEnumerator JumpCorr()
    {
        jumpEnable = false;
        yield return new WaitForSeconds(2f);
        jumpEnable = true;
    }
    
    #endregion
    
    #region Rotation
    
    void RotatePlayer()
    {
        if (cameraPosition == null) return;
        
        if (rotateWithCamera)
        {
            // Le personnage tourne TOUJOURS avec la caméra (même immobile)
            Quaternion targetRotation = Quaternion.Euler(0, cameraPosition.GetRotationX(), 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed * Time.deltaTime);
        }
        else
        {
            // Ancien comportement : ne tourner que si le personnage se déplace
            if (moveDirection.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed * Time.deltaTime);
            }
        }
    }
    
    #endregion
    
    #region Interaction
    
    void InitUI()
    {
        playerUI.UpdateLife(playerShooter.lifePoints, playerShooter.maxLifePoints);
        playerUI.UpdateDamage(playerShooter.damage);
        playerUI.UpdateReload(playerShooter.shootDelay);
        playerUI.UpdateSpeed(playerShooter.speed);
    }
    
    public void ApplyHeal(int value)
    {
        playerShooter.lifePoints += value;
        if (playerShooter.lifePoints >= playerShooter.maxLifePoints) 
            playerShooter.lifePoints = playerShooter.maxLifePoints;
        playerUI.UpdateLife(playerShooter.lifePoints, playerShooter.maxLifePoints);
    }
    
    public void ApplyDamage(int value)
    {
        playerShooter.lifePoints -= value;
        playerUI.UpdateLife(playerShooter.lifePoints, playerShooter.maxLifePoints);
        
        if (playerShooter.lifePoints <= 0)
        {
            animator.SetTrigger("Dying");
            StartCoroutine(DyeCoroutine());
        }
    }
    
    IEnumerator DyeCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
    
    public void Shoot()
    {
        // Point de départ du tir (position du joueur)
        Vector3 shootOrigin = transform.position + Vector3.up * 1.5f+transform.forward;
        
        // Obtenir le centre de l'écran pour viser
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        Vector3 targetPoint;
        RaycastHit hit;
        
        // Raycast pour trouver où le joueur vise
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            // Le joueur vise quelque chose
            targetPoint = hit.point;
        }
        else
        {
            // Le joueur vise dans le vide, utiliser un point lointain
            targetPoint = ray.GetPoint(1000f);
        }
        
        // Calculer la direction du tir
        Vector3 shootDirection = -(targetPoint - shootOrigin).normalized;
        
        // Créer le projectile
        Bullet bull = Instantiate(bulletPrefab, shootOrigin, Quaternion.LookRotation(shootDirection)).GetComponent<Bullet>();
        bull.Initialize(playerShooter.damage, playerShooter.bullSpeed);
        
        animator.SetTrigger("Shoot");
        
    }
    
    IEnumerator ShootCorr()
    {
        shootEnable = false;
        float shootTime = 0f;
        while (shootTime < playerShooter.shootDelay)
        {
            yield return new WaitForEndOfFrame();
            shootTime += Time.deltaTime;
        }
        shootEnable = true;
    }
    
    public void EnhanceDamage()
    {
        playerShooter.damage++;
        playerUI.UpdateDamage(playerShooter.damage);
    }
    
    public void EnhanceReload()
    {
        if (playerShooter.shootDelay > 0.5f)
        {
            playerShooter.shootDelay -= 0.1f;
        }
        playerUI.UpdateReload(playerShooter.shootDelay);
    }
    
    public void EnhanceSpeed()
    {
        playerShooter.speed++;
        playerUI.UpdateSpeed(playerShooter.speed);
    }
    
    #endregion
}