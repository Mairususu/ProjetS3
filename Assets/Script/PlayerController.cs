using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Références")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Shooter playerShooter;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private Transform bulletSpawn;
    
    [Header("Déplacement")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Combat")]
    [SerializeField] private float verticalAimCorrection = 0.1f; // Correction verticale de la visée (0 = pas de correction, 0.2 = vise plus haut)
    
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
        if (cameraManager == null)
        {
            cameraManager = FindObjectOfType<CameraManager>();
            if (cameraManager != null)
            {
                cameraManager.SetTarget(transform);
            }
        }
        InitUI();
    }

    public void SetUI(PlayerUI playerUI)
    {
        this.playerUI = playerUI;
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
        //Inputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 forward = cameraManager.GetForwardDirection();
        Vector3 right = cameraManager.GetRightDirection();
        
        // Déplacement
        moveDirection = (forward * vertical + right * horizontal).normalized;
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? 
            playerShooter.speed + playerShooter.sprint : playerShooter.speed;
        Vector3 move = moveDirection * currentSpeed;
        characterController.Move(move * Time.deltaTime);
        
        // Saut
        if (Input.GetButtonDown("Jump") && jumpEnable)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            StartCoroutine(JumpCorr());
        }
        
        // Tir
        if (Input.GetMouseButtonDown(0) && shootEnable)
        {
            Shoot();
            StartCoroutine(ShootCorr());
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        
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
        if (cameraManager == null) return;
        if (rotateWithCamera)
        {
            Quaternion targetRotation = Quaternion.Euler(0, cameraManager.GetRotationX(), 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed * Time.deltaTime);
        }
        else
        {
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
        if(playerShooter.lifePoints <= 0) playerShooter.lifePoints = 0;
        playerUI.UpdateLife(playerShooter.lifePoints, playerShooter.maxLifePoints);
        if (playerShooter.lifePoints <= 0)
        {
            animator.SetTrigger("Dying");
            Game.Instance.PlayerDie();
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
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        
        if (bulletSpawn == null)
        {
            GameObject spawnPoint = new GameObject("BulletSpawn");
            bulletSpawn = spawnPoint.transform;
            bulletSpawn.SetParent(transform);
            bulletSpawn.localPosition = new Vector3(0, 1.5f, 0.5f);
        }
        
        Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Vector3 targetPoint;
        RaycastHit hit;
        
        Vector3 rayStart = ray.origin + ray.direction * 2f; 
        if (Physics.Raycast(rayStart, ray.direction, out hit, 1000f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 1000f;
        }
        
        // VÉRIFICATION : Si le target est derrière le bulletSpawn, utiliser la direction de la caméra
        Vector3 shootDirection = (targetPoint - bulletSpawn.position).normalized;
        if (Vector3.Dot(shootDirection, transform.forward) < 0)
        {
            shootDirection = ray.direction;
        }
        
        // CORRECTION VERTICALE : Ajuster légèrement la direction vers le haut
        if (verticalAimCorrection != 0)
        {
            Vector3 horizontalDir = new Vector3(shootDirection.x, 0, shootDirection.z).normalized;
            float currentVertical = shootDirection.y;
            float newVertical = currentVertical + verticalAimCorrection;
            
            shootDirection = new Vector3(
                horizontalDir.x * Mathf.Sqrt(1 - newVertical * newVertical),
                newVertical,
                horizontalDir.z * Mathf.Sqrt(1 - newVertical * newVertical)
            ).normalized;
        }
        Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);
         Instantiate(bulletPrefab, bulletSpawn.position, bulletRotation);
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