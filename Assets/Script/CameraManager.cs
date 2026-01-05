using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera cam;
    
    [Header("Distance et Position")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private bool checkCollisions = true;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionLayers = -1;
    
    [Header("Sensibilité")]
    [SerializeField] private float mouseSensitivityX = 3f;
    [SerializeField] private float mouseSensitivityY = 3f;
    [SerializeField] private float scrollSensitivity = 3f;
    
    [Header("Limites de Rotation")]
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    // Variables privées
    private float rotationX = 0f;
    private float rotationY = 20f;
    private float smoothRotationX = 0f;
    private float smoothRotationY = 20f;
    private float currentDistance;
    private float targetDistance;
    
    void Start()
    {
        if (cam == null)
            cam = Camera.main;
        
        if (cam != null)
        {
            Vector3 angles = cam.transform.eulerAngles;
            rotationX = angles.y;
            rotationY = angles.x;
            smoothRotationX = rotationX;
            smoothRotationY = rotationY;
        }
        
        currentDistance = distance;
        targetDistance = distance;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null || cam == null) return;
        
        HandleCameraInput();
        CalculateCameraPosition();
    }
    
    void HandleCameraInput()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
            
            rotationX += mouseX;
            rotationY -= mouseY;
            
            // Limiter l'angle vertical
            rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetDistance -= scroll * scrollSensitivity;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Reverrouiller avec clic droit
        if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void CalculateCameraPosition()
    {
        // Lisser la rotation
        smoothRotationX = Mathf.Lerp(smoothRotationX, rotationX, 10f * Time.deltaTime);
        smoothRotationY = Mathf.Lerp(smoothRotationY, rotationY, 10f* Time.deltaTime);
        
        // Lisser la distance
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, 10f* Time.deltaTime);
        
        // Calculer la rotation en quaternion
        Quaternion rotation = Quaternion.Euler(smoothRotationY, smoothRotationX, 0);
        
        // Position du pivot (au-dessus du joueur)
        Vector3 pivotPosition = target.position + Vector3.up * 1.5f;
        
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
                desiredPosition = pivotPosition + direction * (hit.distance - collisionRadius);
            }
        }
        cam.transform.position = desiredPosition;
        cam.transform.LookAt(pivotPosition);
    }
    
    #region Méthodes publiques
    
    public float GetRotationX()
    {
        return rotationX;
    }
    
    public Vector3 GetForwardDirection()
    {
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    
    public Vector3 GetRightDirection()
    {
        Vector3 right = cam.transform.right;
        right.y = 0;
        return right.normalized;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    
    #endregion
    
    
}
