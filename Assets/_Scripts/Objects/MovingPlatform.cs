using System.Security.Cryptography;
using DG.Tweening;
using UnityEngine;


public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _endTransform;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private LineRenderer _lineRenderer;
    [ColorUsage(true, true)] [SerializeField] private Color _pathColor = Color.white; 
    [SerializeField] private float _lineWidth = 0.15f;
    
    private Rigidbody2D _rb;
    private Transform _currentTarget;

    public Vector2 DeltaPos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if(_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.useFullKinematicContacts = true; 
        
        transform.position = _startTransform.position;
        _currentTarget = _endTransform;
    }

    private void Start()
    {
        if (_lineRenderer != null && _startTransform != null && _endTransform != null)
        {
            _lineRenderer.SetPosition(0, _startTransform.position);
            _lineRenderer.SetPosition(1, _endTransform.position);
            _lineRenderer.startWidth = _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.startColor = _pathColor;
            _lineRenderer.endColor = _pathColor;

        }
    }

    private void FixedUpdate()
    {
        Vector2 newPos = Vector2.MoveTowards(_rb.position, _currentTarget.position, _moveSpeed * Time.fixedDeltaTime);
        DeltaPos = newPos - _rb.position;
        _rb.MovePosition(newPos);

        if (Vector2.Distance(_rb.position, _currentTarget.position) < 0.05f)
        {
            _currentTarget = (_currentTarget == _startTransform) ? _endTransform : _startTransform;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_startTransform.position, _endTransform.position);
        Gizmos.DrawWireCube(_startTransform.position, transform.localScale);
        Gizmos.DrawWireCube(_endTransform.position, transform.localScale);
    }
}