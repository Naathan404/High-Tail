using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private float _lookOffset = 3f;
    [SerializeField] private float _lookDuration = 0.5f;
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private CinemachineCamera _cineCam;

    private PlayerControls _controls;
    private CinemachineFollow _followComponent;
    private float _targetYOffset;
    private Tween _lookTween;

    private void Awake()
    {
        _controls = new PlayerControls();
        _followComponent = _cineCam.GetComponent<CinemachineFollow>();

        _controls.Camera.Look.performed += OnLookPerformed;
        _controls.Camera.Look.canceled += OnLookCanceled;  
    }

    private void OnEnable() => _controls.Camera.Enable();
    private void OnDisable() => _controls.Camera.Disable();

    private void Update()
    {
        bool isMoving = Mathf.Abs(_playerRb.linearVelocityX) > 0.1f || Mathf.Abs(_playerRb.linearVelocityY) > 0.1f;
        // If the player starts moving and the camera is currently offset, reset it to default
        if (isMoving && !Mathf.Approximately(_targetYOffset, 0)) 
        {
            StartLookTween(0);
        }
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        bool isMoving = Mathf.Abs(_playerRb.linearVelocityX) > 0.01f || Mathf.Abs(_playerRb.linearVelocityY) > 0.01f;

        if (!isMoving)
        {
            float input = context.ReadValue<float>();
            float targetY = (input > 0) ? _lookOffset : -_lookOffset;
            StartLookTween(targetY);
        }
    }

    // Return the camera to its default position when the look input is released
    private void OnLookCanceled(InputAction.CallbackContext context) 
    {
        StartLookTween(0);
    }

    private void StartLookTween(float targetY)
    {
        // If the target Y offset is already at the desired value, no need to start a new tween
        if (Mathf.Approximately(_targetYOffset, targetY)) return;

        _targetYOffset = targetY;
        _lookTween?.Kill();

        _lookTween = DOTween.To(() => _followComponent.FollowOffset.y,
                                x => _followComponent.FollowOffset.y = x,
                                targetY, _lookDuration)
                            .SetEase(Ease.InOutSine);
    }
}