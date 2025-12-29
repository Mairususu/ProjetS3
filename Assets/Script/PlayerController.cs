using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera cam;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    [Header("Stats plzyer")]
    [SerializeField] private Shooter playerShooter;
    [Header("Déplacement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Distance et Position Caméra")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float heightOffset = 1.5f;
    
    
    [Header("Rotation du personnage")]
    [SerializeField] private float playerRotationSpeed = 10f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    [Header("Lissage")]
    [SerializeField] private float rotationSmoothing = 12f;
    
    [Header("Collision Caméra")]
    [SerializeField] private bool checkCollisions = true;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionLayers = -1;
    
    // Variables privées - Caméra
    private float rotationX = 0f;
    private float rotationY = 20f;
    private float smoothRotationX = 0f;
    private float smoothRotationY = 20f;
    private float currentDistance;
    private float targetDistance;
    
    // Variables privées - Déplacement
    private Vector3 velocity;
    private bool jumpEnable=true;
    private bool shootEnable=true;
    private Vector3 moveDirection;
    
    void Start()
    {
        // Récupérer le CharacterController si non assigné
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("CharacterController manquant ! Ajoute un CharacterController au joueur.");
            }
        }
        
        // Initialisation caméra
        currentDistance = distance;
        targetDistance = distance;
        
        if (cam != null)
        {
            Vector3 angles = cam.transform.eulerAngles;
            rotationX = angles.y;
            rotationY = angles.x;
            smoothRotationX = rotationX;
            smoothRotationY = rotationY;
        }
        
        // Verrouiller le curseur
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
    }
    
    void LateUpdate()
    {
        RotatePlayer();
        CalculateCameraPosition();
    }
    
    void HandleInput()
    {
        // Rotation de la caméra avec la souris
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * 3f;
            float mouseY = Input.GetAxis("Mouse Y") * 3f;
            
            rotationX += mouseX;
            rotationY -= mouseY;
            
            // Limiter l'angle vertical
            rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
        }
        
        // Zoom avec la molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetDistance -= scroll * 3f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        
        // Déverrouiller le curseur avec ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Reverrouiller avec clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonDown(1)&& shootEnable)
        {
            Shoot();
            StartCoroutine(ShootCorr());
        }
    }

    #region Movement

    

    void HandleMovement()
    {
        if (characterController == null) return;
        
        
        
        // Réinitialiser la vélocité verticale si au sol
        if (velocity.y < 0 && jumpEnable)
        {
            velocity.y = 0;
        }
        
        // Récupérer les inputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculer la direction de déplacement basée sur la caméra
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        
        // Ignorer la composante verticale
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        
        // Direction de déplacement
        moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Vitesse (marche ou course)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // Appliquer le déplacement horizontal
        Vector3 move = moveDirection * currentSpeed;
        characterController.Move(move * Time.deltaTime);
        
        // Saut
        if (Input.GetButtonDown("Jump") && jumpEnable)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            StartCoroutine(JumpCorr());
        }
        
        // Appliquer la gravité
        velocity.y += gravity*Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        if (horizontal == 0f && vertical == 0f)
        {
            animator.SetBool("isMoving", false);
        }
        else
        {
            animator.SetBool("isMoving", true);
        }
    }

    IEnumerator JumpCorr()
    {
        jumpEnable = false;
        float jumpTime = 0f;
        while (jumpTime < 2f)
        {
            yield return new WaitForEndOfFrame();
            jumpTime+= Time.deltaTime;
            Debug.Log("Jump Time: " + jumpTime);
        }

        jumpEnable = true;
    }
    #endregion
    
    #region Rotation et Caméra
    void RotatePlayer()
    {
        // Ne tourner le personnage que s'il se déplace
        if (moveDirection.magnitude > 0.1f)
        {
            // Calculer l'angle de rotation basé sur la direction de déplacement
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            
            // Appliquer la rotation au joueur de manière lissée
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerRotationSpeed * Time.deltaTime);
        }
    }
    
    void CalculateCameraPosition()
    {
        if (cam == null) return;
        
        // Lisser la rotation
        smoothRotationX = Mathf.Lerp(smoothRotationX, rotationX, rotationSmoothing * Time.deltaTime);
        smoothRotationY = Mathf.Lerp(smoothRotationY, rotationY, rotationSmoothing * Time.deltaTime);
        
        // Lisser la distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, 10f * Time.deltaTime);
        
        // Calculer la rotation en quaternion
        Quaternion rotation = Quaternion.Euler(smoothRotationY, smoothRotationX, 0);
        
        // Position du pivot (au-dessus du joueur)
        Vector3 pivotPosition = transform.position + Vector3.up * heightOffset;
        
        // Direction de la caméra (toujours derrière)
        Vector3 direction = rotation * -Vector3.forward;
        
        // Position désirée de la caméra
        Vector3 desiredPosition = pivotPosition + direction * currentDistance;
        
        // Vérifier les collisions
        if (checkCollisions)
        {
            RaycastHit hit;
            if (Physics.SphereCast(pivotPosition, collisionRadius, direction, out hit, currentDistance, collisionLayers))
            {
                // Ajuster la distance si obstacle détecté
                desiredPosition = pivotPosition + direction * (hit.distance - collisionRadius);
            }
        }
        
        // Appliquer la position et la rotation
        cam.transform.position = desiredPosition;
        cam.transform.LookAt(pivotPosition);
    }
    #endregion
    

    #region Interaction

    public void ApplyDamage(int value)
    {
        playerShooter.ApplyDamage(value);
        
        animator.SetTrigger(Animator.StringToHash("Dying"));
    }
    

    public void ApplyHeal(int value)
    {
        playerShooter.ApplyHeal(value);
    }

    public void Shoot()
    {
        playerShooter.Shoot();
        animator.SetTrigger("Shoot");
    }

    IEnumerator ShootCorr()
    {
        shootEnable = false;
        float shootTime = 0f;
        while (shootTime < playerShooter.shootDelay)
        {
            yield return new WaitForEndOfFrame();
            shootTime+= Time.deltaTime;
            Debug.Log("Shoot Time: " + shootTime);
        }

        shootEnable = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
    #endregion
}