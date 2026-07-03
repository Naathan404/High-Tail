using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("Crumbling Settings")]
    [SerializeField] private float _recoveryTime = 3f;
    [SerializeField] private float _brokenDelay = 2f; // Thoi gian delay tu khi cham chan vao platform den khi bat dau roi
    [SerializeField] private ParticleSystem _breakEffect;

    private Renderer _renderer;
    private Collider2D _collider;
    private Trigger _trigger;
    private bool _isSteppedOn;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _trigger = GetComponentInChildren<Trigger>();
    }

    private void OnEnable()
    {
        if (_trigger != null)
            _trigger.OnStepIn += Break;

        DeathScreenManager.OnDeathScreenTriggered += Reset;
    }

    private void OnDisable()
    {
        if (_trigger != null)
            _trigger.OnStepIn -= Break;

        DeathScreenManager.OnDeathScreenTriggered -= Reset;
    }

    private void Break()
    {
        if (_isSteppedOn) return;
        StartCoroutine(BreakCoroutine());
    }

    private IEnumerator BreakCoroutine() // BreakCoroutine the platform -> disappear -> Reset
    {
        // Shaking effect
        Shake();

        yield return new WaitForSeconds(_brokenDelay);

        // Spawn particle
        if (_breakEffect != null)
        {
            _breakEffect.transform.position = transform.position;
            _breakEffect.Play();
        }

        _isSteppedOn = true;
        _renderer.enabled = false;
        _collider.enabled = false;
        //SetChildrenActive(false);
        StartCoroutine(ResetPlatform());
    }

    private void Shake()
    {
        // Shaking
        this.transform.DOShakePosition(_brokenDelay, 0.1f, 10, 90f, false, false);
    }

    private void Reset()
    {
        _isSteppedOn = false;
        _renderer.enabled = true;
        _collider.enabled = true;
        SetChildrenActive(true);
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