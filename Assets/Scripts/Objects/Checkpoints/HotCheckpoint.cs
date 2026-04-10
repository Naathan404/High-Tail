using System;
using UnityEngine;

public class HotCheckpoint : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Transform của vị trí sẽ hồi sinh khi được checkpoint ở đây")]
    [SerializeField] private Transform _respawnPos;
    [SerializeField] private bool _isFacingRight;

    [Header("Debug")]
    [SerializeField] private SpriteRenderer _sr;

    public static event Action<HotCheckpoint> OnStepIn;

    private void Awake()
    {
        if(_sr == null)
            _sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        OnStepIn += ToggleColor;
    }

    private void OnDisable()
    {
        OnStepIn -= ToggleColor;
    }

    // event that player trigger this checkpoint
    public static event Action<Transform, bool> OnTriggerNewCheckpoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            OnTriggerNewCheckpoint?.Invoke(_respawnPos, _isFacingRight);
            OnStepIn?.Invoke(this);
        }
    }

    private void ToggleColor(HotCheckpoint sender)
    {
        if(sender == this)
        {
            _sr.color = Color.blueViolet;
            return;
        }

        _sr.color = Color.whiteSmoke;
    }
}
