using System.Security.Cryptography;
using UnityEngine;


public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform _startPosition;
    [SerializeField] private Transform _endPosition;
    [SerializeField] private float _moveSpeed;
    private Vector2 _lastPosition;
    
    private Rigidbody2D _rb;
    private Transform _currentTarget;

    public Vector2 DeltaPos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.useFullKinematicContacts = true; 
        
        transform.position = _startPosition.position;
        _currentTarget = _endPosition;
    }

    private void Start()
    {
         _lastPosition = _rb.position;
    }

    private void FixedUpdate()
    {
        Vector2 newPos = Vector2.MoveTowards(_rb.position, _currentTarget.position, _moveSpeed * Time.fixedDeltaTime);
        DeltaPos = newPos - _rb.position;
        _rb.MovePosition(newPos);
        _lastPosition = newPos;

        if (Vector2.Distance(_rb.position, _currentTarget.position) < 0.05f)
        {
            _currentTarget = (_currentTarget == _startPosition) ? _endPosition : _startPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_startPosition.position, _endPosition.position);
        Gizmos.DrawWireCube(_startPosition.position, transform.localScale);
        Gizmos.DrawWireCube(_endPosition.position, transform.localScale);
    }
}