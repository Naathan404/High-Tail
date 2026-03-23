using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("Crumbling Settings")]
    [SerializeField] private float _recoveryTime = 3f;
    [SerializeField] private float _platformDurability = 2f; // Do ben cua platform
    [SerializeField] private float _castDistance = 0.2f;
    [SerializeField] private LayerMask _castLayer;

    private float _durability; // Counter
    private Vector2 _boxSize;
    private bool _isBroken = false;

    private Renderer _renderer;
    private Collider2D _collider;
    private Animator _animator;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        _durability = _platformDurability;

        float width = _collider.bounds.size.x;
        _boxSize = new Vector2(width, 0.1f);
    }

    private void Update()
    {
        if (_isBroken) return;

        if (IsPlayerLanding())
        {
            HandleCrumbling();
        }
        else
        {
            Recovery();
        }
    }

    private bool IsPlayerLanding() // Player is landing on the platform
    {
        // The surface of the platform
        Vector2 origin = (Vector2)(_collider.bounds.center + Vector3.up * _collider.bounds.size.y / 2);
        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            _boxSize,
            0f,
            Vector2.up,
            _castDistance,
            _castLayer
        );
        return hit.collider != null;
    }

    private void HandleCrumbling()
    {
        if (_animator != null) 
            _animator.SetBool("isSteppedOn", true);

        _durability -= Time.deltaTime;
        Debug.Log($"Platform is crumbling... Remain time: {_durability:F2}s");

        if (_durability <= 0)
        {
            Break();
        }
    }

    private void Break() // Break the platform -> disappear -> Reset
    {
        _renderer.enabled = false;
        _collider.enabled = false;
        _isBroken = true;
        SetChildrenActive(false);
        StartCoroutine(ResetPlatform());
    }

    private void Recovery() // Recovery the durability by an amount per time, idk if its necessary
    {
        //Recovery
    }

    private IEnumerator ResetPlatform()
    {
        yield return new WaitForSeconds(_recoveryTime);

        // Reset
        _renderer.enabled = true;
        _collider.enabled = true;
        _isBroken = false;
        _durability = _platformDurability;
        SetChildrenActive(true);
    }

    private void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform) 
            child.gameObject.SetActive(isActive);
    }

    private void OnDrawGizmos()
    {
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_collider == null) return;

        Vector3 origin = _collider.bounds.center + Vector3.up * (_collider.bounds.size.y / 2);
        Vector3 endPos = origin + Vector3.up * _castDistance;

        // Start pos
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(origin, new Vector3(_boxSize.x, _boxSize.y, 0.1f));

        // End pos
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(endPos, new Vector3(_boxSize.x, _boxSize.y, 0.1f));

        // Connect line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, endPos);
    }
}