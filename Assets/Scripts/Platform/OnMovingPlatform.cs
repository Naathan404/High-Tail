using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEditor.Experimental.GraphView;
using TreeEditor;
// dmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm

public class OnMovingPlatform : MonoBehaviour
{
    [Header("Platform settings")]
    [SerializeField] private float _movingTime = 0.5f;
    [SerializeField] private float _returnTime = 0.5f;
    [Tooltip("Chose if the platform should return to original position after player died")]
    [SerializeField] private bool _isReset;

    private Vector3 _originalPosition;
    private Vector3 _targetPosition;
    private Trigger _trigger;
    private GlideTrigger _glideTrigger;
    private bool _isMoving;

    // Sync velocity
    private PlayerController _controller;
    private Vector3 _lastPosition;
    private bool _isPlatformAtTarget;
    private bool _isPlayerOnPlatform;

    private void Awake()
    {
        _originalPosition = transform.position;
        Transform targetTransform = transform.Find("TargetPosition");
        _targetPosition = targetTransform.position;

        _trigger = GetComponentInChildren<Trigger>();
        _glideTrigger = GetComponentInChildren<GlideTrigger>();

        _controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        _lastPosition = transform.position;
    }

    private void OnEnable()
    {
        if (_trigger == null)
        {
            Debug.LogWarning("Trigger is null");
            return;
        }
        if (_glideTrigger == null)
        {
            Debug.LogWarning("GlideTrigger is null");
            return;
        }
        if (_controller == null)
        {
            Debug.LogWarning("PlayerController is null");
            return;
        }

        _trigger.OnStepIn += MoveToTarget;

        _glideTrigger.OnGlideStart += MoveToTarget;
        //_glideTrigger.OnGlideUpdate += SyncPosition;
        _glideTrigger.OnGlideEnd += ResetPhysics;

        _controller.OnPlayerDied += ResetPlatform;
    }

    private void OnDisable()
    {
        _trigger.OnStepIn -= MoveToTarget;

        _glideTrigger.OnGlideStart -= MoveToTarget;
        //_glideTrigger.OnGlideUpdate -= SyncPosition;
        _glideTrigger.OnGlideEnd -= ResetPhysics;

        _controller.OnPlayerDied -= ResetPlatform;
    }

    private void LateUpdate()
    {
        SyncPosition();

        _lastPosition = transform.position;
    }

    private void MoveToTarget()
    {
        if (_isMoving) return;

        _isMoving = true;
        _isPlatformAtTarget = false;

        transform.DOKill();
        transform.DOMove(_targetPosition, _movingTime)
                 .SetEase(Ease.InOutSine)
                 .OnComplete(() =>
                 {
                     _isPlayerOnPlatform = false;
                     _isPlatformAtTarget = true;
                     ResetPhysics();
                 });
    }

    private void SyncPosition()
    {
        // If the player is touching the platform (standing on it or gliding) and the platform hasn't reach the target yet,
        // it will sync the player position with the platform position
        if (_isPlatformAtTarget) return;

        // Sync position
        if (_isPlayerOnPlatform || _glideTrigger.onWallGlideState)
        {
            Vector3 delta = transform.position - _lastPosition;
            _controller.transform.position += delta;
        }

        // Set player's gravity and velocity -> the player will not affect by gravity
        if (_glideTrigger.onWallGlideState)
        {
            _controller.Rb.gravityScale = 0f;
            _controller.Rb.linearVelocity = Vector2.zero;
        }
    }

    private void ResetPlatform() // Move the platform back to original position when player died
    {
        if (_isReset == false) return;

        _isMoving = true;
        _isPlatformAtTarget = false;

        transform.DOKill();
        transform.DOMove(_originalPosition, _returnTime)
                 .SetEase(Ease.OutQuart)
                 .OnUpdate(() =>
                 {
                     _lastPosition = transform.position;
                 })
                 .OnComplete(() =>
                 {
                     _isMoving = false;
                     _lastPosition = transform.position;
                 });
    }

    private void ResetPhysics() // Reset player's gravity and add a small force to make sure the player falls down
    {
        if (_controller == null)
        {
            Debug.LogWarning("PlayerController is null");
            return;
        }
        _glideTrigger.onWallGlideState = false;

        if (_controller.Rb.gravityScale == 0f)
        {
            // Neu sau khi cham target ma van con truot
            if (_glideTrigger.onWallGlideState)
            {
                _controller.Rb.gravityScale = _controller.Data.wallSlideGravityScale;
            }
            // Ko con truot
            else
            {
                _controller.Rb.gravityScale = _controller.BaseGravity;
            }

            _controller.Rb.WakeUp();
            // Add a small force 
            _controller.Rb.linearVelocity = new Vector2(_controller.Rb.linearVelocity.x, -0.1f);
        }
    }

    #region CheckCondition
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    _isPlayerOnPlatform = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerOnPlatform = false;
            ResetPhysics();
        }
    }
    #endregion
}