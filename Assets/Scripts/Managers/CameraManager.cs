using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [SerializeField] private float _lookOffset = 3f;
    [SerializeField] private float _lookDuration = 0.5f;
    [SerializeField] private float _timeFreezeStrength = 0.01f;
    [SerializeField] private float _timeFreezeDuration = 0.05f;
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private CinemachineCamera _cineCam;
    private CinemachineFollow _followComponent;
    private CinemachineConfiner2D _confinerComponent;
    private float _targetYOffset;
    private Tween _lookTween;

    private void Awake()
    {
        if(Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
        _followComponent = _cineCam.GetComponent<CinemachineFollow>();
        _confinerComponent = _cineCam.GetComponent<CinemachineConfiner2D>();

    }

    private void Start()
    {
        InputManager.Instance.Inputs.Camera.Look.performed += OnLookPerformed;
        InputManager.Instance.Inputs.Camera.Look.canceled += OnLookCanceled;  
    }

    private void OnDestroy()
    {
        InputManager.Instance.Inputs.Camera.Look.performed -= OnLookPerformed;
        InputManager.Instance.Inputs.Camera.Look.canceled -= OnLookCanceled;       
    }



    private void Update()
    {
        bool isMoving = Mathf.Abs(_playerRb.linearVelocityX) > 0.1f || Mathf.Abs(_playerRb.linearVelocityY) > 0.1f;
        // If the player starts moving and the camera is currently offset, reset it to default
        if (isMoving && !Mathf.Approximately(_targetYOffset, 0)) 
        {
            StartLookTween(0);
        }
    }

    // --- HÀM CHUYỂN PHÒNG: Vẫn Follow nhưng đổi biên giới ---
    [System.Obsolete]
    public void SwitchRoom(BoxCollider2D newRoomCollider)
    {
        if (_confinerComponent.BoundingShape2D == newRoomCollider) return;

        // cập nhật vùng camera mới
        _confinerComponent.BoundingShape2D = newRoomCollider;
        _confinerComponent.InvalidateCache();

        // 3. Hiệu ứng khựng nhẹ của Celeste để tạo cảm giác sang vùng mới
        StartCoroutine(RoomTransitionFreeze());
    }

    private IEnumerator RoomTransitionFreeze()
    {
        GameManager.Instance.DoTimeFreeze(_timeFreezeStrength, _timeFreezeDuration);
        yield return new WaitForSecondsRealtime(0.1f);
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