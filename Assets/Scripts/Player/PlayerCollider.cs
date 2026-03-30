using Unity.Cinemachine;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    #region Collision
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {        
        if(collision.gameObject.TryGetComponent<IHarmful>(out IHarmful damager))
        {
            KillPlayer();
            CameraShaker.Instance.OneTimeShake(Vector2.right, 0.5f);
            GameManager.Instance.DoTimeFreeze(0, 0.1f);
            damager.DealDamage();
            ApplyKnockback(damager.Knockback);
        }
        if (collision.gameObject.CompareTag("RotatingPlatform"))
        {
            transform.SetParent(collision.transform);
        }        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<IBouncy>(out IBouncy bouncer))
        {
            _stateMachine.ChangeState(IdleState);
            ApplyBounce(bouncer.BouncyForce);
            bouncer.PlayBounceAnimation();   
        }

        if(collision.gameObject.TryGetComponent<IPushable>(out IPushable pusher))
        {
            _stateMachine.ChangeState(IdleState);
            ApplyPush(pusher.PushForce, pusher.IsRight);
            pusher.PlayPushAnimation();
        }

        if(collision.gameObject.TryGetComponent<VineSegment>(out VineSegment segment))
            {
                if (_vineGrabCooldownTimer <= 0f)
                {
                    CurrentVineRb = segment.Rb;
                    CurrentVineTransform = segment.transform;

                    if(segment.Type == VineType.LooseSwing)
                    {
                        _stateMachine.ChangeState(VineSwingState);
                    }
                    else
                    {
                        _stateMachine.ChangeState(VineClimbState);
                    }
                }
            }

        if (collision.gameObject.TryGetComponent<Trigger>(out Trigger trigger))
        {
            trigger.ExecuteTrigger();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("RotatingPlatform"))
        {
            transform.SetParent(null);
        }
    }
    #endregion
}
