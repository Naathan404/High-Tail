using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightRotatingPlatform : MonoBehaviour, ILightPulseReactive
{
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 45f; 
    [SerializeField] private bool _isClockwise = true;  

    [SerializeField] private bool _isActivated = false;
    
    // THÊM: Trạng thái đang "rà trạm" để dừng
    private bool _isStopping = false; 
    private float _targetAngle = 0f;

    private float _originalRotationZ; 
    private Rigidbody2D _rb;

    private void OnEnable()
    {
        DeathScreenManager.OnDeathScreenTriggered += Reset;
    }

    private void OnDisable()
    {
        DeathScreenManager.OnDeathScreenTriggered -= Reset;
    }

    public void ReactToLightPulse()
    {
        StartCoroutine(RotateCoroutine());
    }

    private IEnumerator RotateCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        if (CameraShakeManager.Instance != null)
        {
            CameraShakeManager.Instance.ShakeCustom(0.5f);
        }
    
        if (_isActivated)
        {
            _isActivated = false;
            _isStopping = true;

            float currentAngle = _rb.rotation;
            float direction = _isClockwise ? -1f : 1f;

            if (direction > 0) 
            {
                _targetAngle = Mathf.Ceil(currentAngle / 90f) * 90f;

                if (Mathf.Abs(_targetAngle - currentAngle) < 1f) _targetAngle += 90f; 
            }
            else 
            {
                _targetAngle = Mathf.Floor(currentAngle / 90f) * 90f;
                
                if (Mathf.Abs(currentAngle - _targetAngle) < 1f) _targetAngle -= 90f;
            }
        }
        else
        {
            _isActivated = true;
            _isStopping = false; 
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _rb.bodyType = RigidbodyType2D.Kinematic; 
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        _originalRotationZ = _rb.rotation;
    }

    private void Reset()
    {
        _isActivated = false;
        _isStopping = false; 
        
        _rb.rotation = _originalRotationZ;
        transform.rotation = Quaternion.Euler(0, 0, _originalRotationZ);
    }

    private void FixedUpdate()
    {
        if (_isActivated)
        {
            float direction = _isClockwise ? -1f : 1f;
            float newRotation = _rb.rotation + (_rotationSpeed * direction * Time.fixedDeltaTime);
            _rb.MoveRotation(newRotation);
        }
        else if (_isStopping)
        {
            float step = _rotationSpeed * Time.fixedDeltaTime;
            
            float newRotation = Mathf.MoveTowardsAngle(_rb.rotation, _targetAngle, step);
            _rb.MoveRotation(newRotation);

            if (Mathf.Abs(Mathf.DeltaAngle(_rb.rotation, _targetAngle)) < 0.1f)
            {
                CameraShakeManager.Instance.ShakeCustom(0.3f);
                _rb.rotation = _targetAngle; 
                _isStopping = false;
            }
        }
    }
}