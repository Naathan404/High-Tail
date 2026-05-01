using System;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

public class PushMushroom : MonoBehaviour, IPushable
{
    [Header("Settings")]
    [SerializeField] private Transform _visual;
    [SerializeField] private float _pushX = 20f;
    [SerializeField] private float _pushY = 10f;
    
    [SerializeField] private bool _isFacingRight = true;
    [SerializeField] private bool _isShortMushroom = true;
    
    public Vector2 PushForce => new Vector2(_pushX * IsRight, _pushY); 
    
    private Vector3 _originalScale;
    public float IsRight => _isFacingRight ? 1f : -1f;

    private void Start()
    {
        _visual.transform.localScale = new Vector3(_visual.transform.localScale.x * IsRight, _visual.transform.localScale.y, 1f);
        _originalScale = _visual.transform.localScale;
    }

    public void PlayPushAnimation()
    {
        _visual.transform.DOKill();
        _visual.transform.DOScale(new Vector3(1.5f * IsRight, 0.6f), 0.1f)
               .OnComplete(() => _visual.transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutElastic));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if(player == null) return;
            bool isHitFromAbove = collision.transform.position.y > transform.position.y;
            bool isFalling = player.Rb.linearVelocity.y <= 0f;
            if(_isShortMushroom)
            {
                DoPush(player);
            }
            else
            {
                if(isHitFromAbove && isFalling)
                {
                    DoPush(player);
                }
            }
        }
    }

    private void DoPush(PlayerController player)
    {
        PlayPushAnimation();

        player.CanDash = false;
        player.SetFacingDirection(_isFacingRight);
        player.transform.localScale = new Vector2(_isFacingRight ? 1f : -1f, 1f);
        player.Visual.ApplySquashStretch(new Vector3(1.2f, 0.8f, 1f));

        player.Rb.linearVelocity = Vector2.zero;
        player.Rb.linearVelocity = new Vector2(_isFacingRight ? _pushX : _pushX * -1f, _pushY);
        player.StateMachine.ChangeState(player.BounceState);
    }
}