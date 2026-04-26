using UnityEngine;
using System.Collections;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("Crumbling Settings")]
    [SerializeField] private float _recoveryTime = 3f;
    [SerializeField] private float _brokenDelay = 2f; // Thoi gian delay tu khi cham chan vao platform den khi bat dau roi

    private Renderer _renderer;
    private Collider2D _collider;
    private Animator _animator;
    private Trigger _trigger;
    private bool _isSteppedOn;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        _trigger = GetComponentInChildren<Trigger>();
        _animator.speed = 1f / _brokenDelay;
    }

    private void OnEnable()
    {
        if (_trigger != null)
            _trigger.OnStepIn += Break;
    }

    private void OnDisable()
    {
        if (_trigger != null)
            _trigger.OnStepIn -= Break;
    }

    private void Break()
    {
        if (_isSteppedOn) return;
        StartCoroutine(BreakCoroutine());
    }

    private IEnumerator BreakCoroutine() // BreakCoroutine the platform -> disappear -> Reset
    {
        // Turn crumbling animation
        _animator.SetTrigger("Break");

        yield return new WaitForSeconds(_brokenDelay);

        _isSteppedOn = true;
        _renderer.enabled = false;
        _collider.enabled = false;
        SetChildrenActive(false);
        StartCoroutine(ResetPlatform());
    }

    private IEnumerator ResetPlatform()
    {
        yield return new WaitForSeconds(_recoveryTime);

        // Reset
        _isSteppedOn = false;
        _renderer.enabled = true;
        _collider.enabled = true;
        SetChildrenActive(true);
    }

    private void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
}