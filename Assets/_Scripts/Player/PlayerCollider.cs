using Unity.Cinemachine;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    [Header("Poison Damage")]
    [SerializeField] private float _poisonDamageCooldown = 0.5f; //Thời gian nhận sát thương
    private float _lastPoisonTime = 0f;

    #region Collision
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<IHarmful>(out IHarmful damager))
        {
            KillPlayer();
            CameraShakeManager.Instance.ShakeForDamage();
            GameManager.Instance.DoTimeFreeze(0.05f, 0.25f);
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
        // if (collision.gameObject.TryGetComponent<IBouncy>(out IBouncy bouncer))
        // {
        //     _stateMachine.ChangeState(IdleState);
        //     ApplyBounce(bouncer.BouncyForce);
        //     bouncer.PlayBounceAnimation();
        // }

        // if (collision.gameObject.TryGetComponent<IPushable>(out IPushable pusher))
        // {
        //     _stateMachine.ChangeState(IdleState);
        //     ApplyPush(pusher.PushForce, pusher.IsRight);
        //     pusher.PlayPushAnimation();
        // }

        if (collision.gameObject.TryGetComponent<Trigger>(out Trigger trigger))
        {
            trigger.ExecuteTrigger();
        }

        if (collision.gameObject.TryGetComponent<VineSegment>(out VineSegment segment))
        {
            if (!GrabHeld) return;
            bool isAlreadyGrabbing = _stateMachine.CurrentState == VineSwingState || _stateMachine.CurrentState == VineClimbState;
            if (isAlreadyGrabbing) return;

            if (_vineGrabCooldownTimer <= 0f)
            {
                CurrentVineRb = segment.Rb;
                CurrentVineTransform = segment.transform;

                if (segment.Type == VineType.LooseSwing)
                {
                    _stateMachine.ChangeState(VineSwingState);
                }
                else
                {
                    _stateMachine.ChangeState(VineClimbState);
                }
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("poison");
        if (other.CompareTag("Poison"))
        {
            if (Time.time >= _lastPoisonTime + _poisonDamageCooldown)
            {
                _lastPoisonTime = Time.time;
                float damageAmount = 0f;
                try
                {
                    PoisonMushroom poison = other.GetComponentInParent<PoisonMushroom>();
                    if (poison != null)
                    {
                        damageAmount = poison.PoisonDamage;
                    }
                    else
                    {
                        Debug.Log("Poison error");
                    }
                }
                catch
                {
                    
                }
                TakePoisonDamage(damageAmount);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("RotatingPlatform"))
        {
            transform.SetParent(null);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Trigger>(out Trigger trigger))
        {
            trigger.ExitTrigger();
        }
    }
    #endregion

    private void TakePoisonDamage(float damage = 0f)
    {
        //TODO: Kết nối với cơ chế Heal/Trừ máu thực tế của cậu ở đây
        Debug.Log($"<color=green>Player trúng độc! Mất {damage} HP </color>");
    }
}
