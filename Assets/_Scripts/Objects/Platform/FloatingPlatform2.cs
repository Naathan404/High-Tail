using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FloatingPlatform2 : MonoBehaviour
{
    [SerializeField] private float _offsetY = 0.5f;
    [SerializeField] private float _offsetX = 0f;
    [SerializeField] private float _floatingDuration = 2f;

    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        // move Y
        this.transform.DOMoveY(this.transform.position.y + _offsetY, _floatingDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        // move X
        this.transform.DOMoveX(this.transform.position.x + _offsetX, _floatingDuration * 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            player.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if(player.StateMachine.CurrentState == player.IdleState || player.StateMachine.CurrentState == player.RunState)
                player.Rb.linearVelocity = new Vector2(player.Rb.linearVelocity.x, _rb.linearVelocity.y); // sync player's Y velocity with the platform's movement
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if(player != null)
            {
                player.ReturnToCoreScene();
            }
        }
    }
}
