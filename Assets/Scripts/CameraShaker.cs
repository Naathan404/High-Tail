using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    // --- Chưa có cơ chế chặn tính năng nhìn khi đang rung ----

    public static CameraShaker Instance;// Singleton

    [SerializeField] private CinemachineCamera _cinemachineCamera;
    private CinemachineFollow _followComponent;
    private Vector3 _originalOffset;
    private Tween _impactTween;
    private Tween _continuousTween;

    // For testing purposes
    private PlayerControls _controls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        _controls = new PlayerControls();
        _followComponent = _cinemachineCamera.GetComponent<CinemachineFollow>();
        _originalOffset = _followComponent.FollowOffset;
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void Update()
    {
        if (_controls.Test.T.WasPressedThisFrame())
        {
            OneTimeShake(Vector3.right, 1f);
        }
        if (_controls.Test.Y.WasPressedThisFrame())
        {
            OneTimeShake(Vector3.up, 2f);
        }
    }

    // Shaking for a single frame, useful for things like gun recoil or small impacts
    public void OneTimeShake(Vector3 direction, float magnitude)
    {
        if (_followComponent == null) return;
        _impactTween?.Kill();

        _impactTween = DOTween.Punch(() => CurrentOffset,
                      x => CurrentOffset = x,
                      direction * magnitude, 0.5f, 10, 0.3f)
                      .OnComplete(ReturnToOriginalOffset);
    }

    // Shaking fo ra specific duration, useful for things like explosions or impacts
    public void TimedShake(Vector3 direction, float magnitude, float duration, bool setEase = false)
    {
        if (_followComponent == null) return;
        _impactTween?.Kill();

        _impactTween = DOTween.Shake(() => CurrentOffset,
                        x => CurrentOffset = x,
                        duration, direction * magnitude, 10, 0, setEase)
                        .OnComplete(ReturnToOriginalOffset);
    }

    // Shaking until stopped, useful for things like earthquakes or continuous explosions
    public void StartShaking(Vector3 direction, float magnitude, bool setEase = false)
    {
        if (_followComponent == null) return;
        _continuousTween?.Kill();

        _continuousTween = DOTween.Shake(() => CurrentOffset,
                        x => CurrentOffset = x,
                       0.7f, direction * magnitude, 10, 0, setEase)
                       .SetLoops(-1, LoopType.Restart);
    }

    public void StopShaking()
    {
        _continuousTween?.Kill();
        ReturnToOriginalOffset();
    }

    private void ReturnToOriginalOffset()
    {
        DOTween.To(() => CurrentOffset,
              x => CurrentOffset = x,
              _originalOffset, 0.15f);
    }

    private Vector3 CurrentOffset
    {
        get => _followComponent.FollowOffset;
        set => _followComponent.FollowOffset = value;
    }
}