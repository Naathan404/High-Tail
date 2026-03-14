using UnityEngine;
using System.Collections;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("Crumbling Settings")]
    [SerializeField] private float _crumbleDelay;
    [SerializeField] private float _recoveryTime;
    // Chỉ tắt collider và renderer thay vi SetActive(false)
    private Renderer _renderer;
    private Collider2D _collider;

    private Animator _animator;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
    }

    // Check if player is on the platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Crumble());
        }
    }

    private void SetChildrenActive(bool isActive)
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }
    private IEnumerator Crumble()
    {
        _animator.SetTrigger("Crumble"); // Just for fun
        yield return new WaitForSeconds(_crumbleDelay);
        SetChildrenActive(false);
        _renderer.enabled = false;
        _collider.enabled = false;
        // Recovery after a delay
        StartCoroutine(Recovery());
    }
    private IEnumerator Recovery()
    {
        yield return new WaitForSeconds(_recoveryTime);
        SetChildrenActive(true);
        _renderer.enabled = true;
        _collider.enabled = true;
    }
}
