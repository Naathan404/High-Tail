using UnityEngine;
using DG.Tweening; // Import DOTween để làm hiệu ứng lún

public class BouncyMushroom : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float _normalBounceForce = 15f; 
    [SerializeField] private float _pogoBounceForce = 25f;

    [Header("Animation Settings")]
    [SerializeField] private Transform _visual;
    [SerializeField] private float _squishDuration = 0.2f;
    [SerializeField] private Vector3 _squishScale = new Vector3(1.4f, 0.6f, 1f);
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = _visual.transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player == null) return;
            bool isHitFromAbove = collision.transform.position.y > transform.position.y;
            bool isFalling = player.Rb.linearVelocity.y <= 0.1f;

            if (isHitFromAbove && isFalling)
            {
                DoBounce(player);
            }
        }
    }

    private void DoBounce(PlayerController player)
    {
        _visual.transform.DOKill();

        _visual.transform.DOScale(_squishScale, _squishDuration / 2f).OnComplete(() =>
        {
            _visual.transform.DOScale(_originalScale, _squishDuration).SetEase(Ease.OutElastic);
        });


        float finalForce = _normalBounceForce;
        if (player.StateMachine.CurrentState == player.PogoState)
        {
            finalForce = _pogoBounceForce;
        }

        player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, finalForce);
        player.CanDash = true; 
    }
}