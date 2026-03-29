using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 45f; 
    [SerializeField] private bool _isClockwise = true;  

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        _rb.bodyType = RigidbodyType2D.Kinematic; 
        _rb.sleepMode = RigidbodySleepMode2D.NeverSleep; 
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        float direction = _isClockwise ? -1f : 1f;
        float newRotation = _rb.rotation + (_rotationSpeed * direction * Time.fixedDeltaTime);
        _rb.MoveRotation(newRotation);
    }
}