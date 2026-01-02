using UnityEngine;

public class Billboard : MonoBehaviour
{
    private enum ModeType
    {
        Default = 0,
        Vertical,
        Static
    }

    [SerializeField] private ModeType _mode;

    private Transform _transform;

    private bool _cameraTransformIsSet;
    private Transform _cameraTransform;
    private Vector3 _initialForward;
    
    protected void LateUpdate()
    {
        if (!_cameraTransformIsSet)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return;

            _cameraTransform = mainCamera.transform;
            _cameraTransformIsSet = true;
            
            ResetTransform(_cameraTransform);
            _transform = transform;
            _initialForward = _transform.forward;

            // Destroy the component if the game object is static
            // to avoid overheads. The object will be align to the camera only once.
            if (gameObject.isStatic)
            {
                Destroy(this);
            }
        }

        switch (_mode)
        {
            case ModeType.Default:
                _transform.forward = _cameraTransform.forward;
                break;
            case ModeType.Vertical:
                var forward = _cameraTransform.forward;
                forward.y = 0f;
                _transform.forward = forward;
                break;
            case ModeType.Static:
                _transform.forward = _initialForward;
                break;
            default:
                _transform.forward = _transform.forward;
                break;
        }
    }
    
    private void ResetTransform(Transform cameraTransform)
    {
        if (_mode == ModeType.Default)
        {
            transform.forward = cameraTransform.forward;
        }
        else if (_mode == ModeType.Vertical)
        {
            var forward = cameraTransform.forward;
            forward.y = 0f;

            transform.forward = forward;
        }
    }
}