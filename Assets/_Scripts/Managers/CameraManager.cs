using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : Singleton<CameraManager>
{
    [Header("Look Settings")]
    [SerializeField] private float _lookOffset = 3f;
    [SerializeField] private float _lookDuration = 0.5f;
    
    [Header("Room Transition")]
    [SerializeField] private float _timeFreezeStrength = 0.01f;
    [SerializeField] private float _timeFreezeDuration = 0.05f;

    [Header("Dynamic Zoom Settings")]
    [SerializeField] private float _speedThreshold = 10f;
    // [SerializeField] private float _zoomLerpSpeed = 3f;

    [Header("Dialogue Zoom Settings")]
    [SerializeField] private float _setUpFOV = 70f;
    [SerializeField] private float _dialogueZoomFOV = 40f; 
    [SerializeField] private float _dialogueTransitionTime = 0.8f; 
    [SerializeField] private float _dialogueYOffset = 0.5f; 
    [SerializeField] private float _dialogueScreenY = 0.35f; 
    private float _defaultScreenY;

    // [Header("TaleStone Zoom Settings")]
    // [SerializeField] private float _taleStoneZoomFOV = 40f; 
    // [SerializeField] private float _taleStoneTransitionTime = 0.8f; 
    // [SerializeField] private float _taleStoneYOffset = 0.5f; 
    // [SerializeField] private float _taleStoneScreenY = 0.35f;     

    [Header("References")]
    [SerializeField] private Rigidbody2D _playerRb;
    [SerializeField] private CinemachineCamera _cineCam;
    [SerializeField] private ShockwaveController _shockWave;
    
    private CinemachinePositionComposer _composerComponent;
    private CinemachineConfiner2D _confinerComponent;
    
    private float _targetYOffset;
    private Tween _lookTween;
    private Sequence _dialogueSequence;

    private float _defaultFOV;
    private Vector3 _defaultTargetOffset; 
    private bool _isInDialogue = false;
    private float _currentFOV;     
    private float _roomBaseFOV;     
    private float _roomDashFOV;     
    private bool _isDynamicRoom;    

    public BoxCollider2D CurrentBoundary;

    // Events
    private void OnEnable()
    {
        AstralGift.OnCollected += _shockWave.CallShockWave;
        AstralPulseSkill.OnPulsed += _shockWave.CallShockWave;
    }

    private void OnDisable()
    {
        AstralGift.OnCollected -= _shockWave.CallShockWave;
        AstralPulseSkill.OnPulsed -= _shockWave.CallShockWave;

    }

    private void Start()
    {
        _composerComponent = _cineCam.GetComponent<CinemachinePositionComposer>();
        _confinerComponent = _cineCam.GetComponent<CinemachineConfiner2D>();

        if (_composerComponent == null)
        {
            Debug.LogError("CineCam không có CinemachinePositionComposer! Hãy kiểm tra lại Inspector.");
        }
        else
        {
            _defaultTargetOffset = _composerComponent.TargetOffset;
        }

        _roomBaseFOV = _cineCam.Lens.FieldOfView;
        _currentFOV = _roomBaseFOV;
        _defaultScreenY = _composerComponent.Composition.ScreenPosition.y;

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
        if (_playerRb == null || _isInDialogue) return;

        bool isMoving = Mathf.Abs(_playerRb.linearVelocityX) > 0.1f || Mathf.Abs(_playerRb.linearVelocityY) > 0.1f;
        if (isMoving && !Mathf.Approximately(_targetYOffset, 0)) 
        {
            StartLookTween(0);
        }

        float targetFOV = _roomBaseFOV; // Mặc định là FOV tĩnh của phòng

        if (_isDynamicRoom)
        {
            // Nếu là phòng Dynamic, FOV thay đổi theo vận tốc
            float speed = Mathf.Abs(_playerRb.linearVelocity.x);
            if (speed > _speedThreshold) 
            {
                targetFOV = _roomDashFOV;
            }
        }
        
        
        // if (!Mathf.Approximately(_cineCam.Lens.FieldOfView, targetFOV))
        // {
        //     var lens = _cineCam.Lens;
        //     lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * _zoomLerpSpeed);
        //     _cineCam.Lens = lens;
        // }     
    }

    public void ForceUpdateRoomSettings(float baseFOV, bool isDynamic, float dashFOV)
    {
        _roomBaseFOV = baseFOV;
        _currentFOV = baseFOV;
        _isDynamicRoom = isDynamic;
        _roomDashFOV = dashFOV;

        var lens = _cineCam.Lens;
        lens.FieldOfView = baseFOV;
        _cineCam.Lens = lens;
    }

    public void ZoomIntoDialogue(Transform npcTransform, System.Action onComplete = null)
    {
        _isInDialogue = true;
        _dialogueSequence?.Kill();
        _lookTween?.Kill(); 

        // tắt confider
        //if (_confinerComponent != null) _confinerComponent.enabled = false;

        
        Vector2 offsetToMidpoint = (npcTransform.position - _playerRb.transform.position) / 2f;
        
        Vector3 targetOffset = new Vector3(
            _defaultTargetOffset.x + offsetToMidpoint.x, 
            _defaultTargetOffset.y + offsetToMidpoint.y + _dialogueYOffset, 
            _defaultTargetOffset.z);

        _dialogueSequence = DOTween.Sequence();
        
        _dialogueSequence.Join(DOTween.To(
            () => _composerComponent.TargetOffset,
            x => _composerComponent.TargetOffset = x,
            targetOffset, 
            _dialogueTransitionTime).SetEase(Ease.InOutCubic));

        _dialogueSequence.Join(DOTween.To(
            () => _cineCam.Lens.FieldOfView,
            x => {
                var lens = _cineCam.Lens;
                lens.FieldOfView = x;
                _cineCam.Lens = lens; 
            },
            _dialogueZoomFOV, 
            _dialogueTransitionTime).SetEase(Ease.InOutCubic));

        _dialogueSequence.Join(DOTween.To(
            () => _composerComponent.Composition.ScreenPosition.y, 
            y => {
                var comp = _composerComponent.Composition;
                comp.ScreenPosition = new Vector2(comp.ScreenPosition.x, y); 
                _composerComponent.Composition = comp;
            },
            _dialogueScreenY, 
            _dialogueTransitionTime).SetEase(Ease.InOutCubic));

        _dialogueSequence.OnComplete(() => {
            onComplete?.Invoke();
        });
    }

    public void ResetDialogueCamera(System.Action onComplete = null)
    {
        _dialogueSequence?.Kill();
        _dialogueSequence = DOTween.Sequence();

        _dialogueSequence.Join(DOTween.To(
            () => _composerComponent.TargetOffset,
            x => _composerComponent.TargetOffset = x,
            _defaultTargetOffset, 
            _dialogueTransitionTime).SetEase(Ease.InOutCubic));

        _dialogueSequence.Join(DOTween.To(
            () => _composerComponent.Composition.ScreenPosition.y,
            y => {
                var comp = _composerComponent.Composition;
                comp.ScreenPosition = new Vector2(comp.ScreenPosition.x, y);
                _composerComponent.Composition = comp;
            },
            _defaultScreenY, 
            _dialogueTransitionTime).SetEase(Ease.InOutCubic));

        // trả lại fov
        _dialogueSequence.Join(DOTween.To(
            () => _cineCam.Lens.FieldOfView,
            x => {
                var lens = _cineCam.Lens;
                lens.FieldOfView = x;
                _cineCam.Lens = lens;
            },
            _currentFOV,
            _dialogueTransitionTime).SetEase(Ease.InOutCubic));
            
        _dialogueSequence.OnComplete(() => {
            _isInDialogue = false;
            
            // bật lại confider
            //if (_confinerComponent != null) _confinerComponent.enabled = true;
            _confinerComponent.BoundingShape2D = CurrentBoundary;
            
            onComplete?.Invoke();
        });
    }

    [System.Obsolete]
    public void SwitchRoom(BoxCollider2D newRoomCollider, float baseFOV, bool isDynamic, float dashFOV, bool isDialogue = false)
    {
        if (_confinerComponent.BoundingShape2D == newRoomCollider) return;
        
        if(!isDialogue)
        {
            _roomBaseFOV = baseFOV;
            _isDynamicRoom = isDynamic;
            _roomDashFOV = dashFOV;
            _currentFOV = baseFOV;
        }

        // Đổi ranh giới và Invalid Cache
        _confinerComponent.BoundingShape2D = newRoomCollider;
        _confinerComponent.InvalidateCache();
        
        StartCoroutine(RoomTransitionFreeze());
    }   

    private IEnumerator RoomTransitionFreeze()
    {
        GameManager.Instance.DoTimeFreeze(_timeFreezeStrength, _timeFreezeDuration);
        yield return new WaitForSecondsRealtime(0.1f);
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        if (_isInDialogue || _composerComponent == null) return; 
        bool isMoving = Mathf.Abs(_playerRb.linearVelocityX) > 0.01f || Mathf.Abs(_playerRb.linearVelocityY) > 0.01f;

        if (!isMoving)
        {
            float input = context.ReadValue<float>();
            float targetY = (input > 0) ? _lookOffset : -_lookOffset;
            StartLookTween(targetY);
        }
    }

    private void OnLookCanceled(InputAction.CallbackContext context) 
    {
        if (_isInDialogue) return;
        StartLookTween(0);
    }

    private void StartLookTween(float targetY)
    {
        if (_composerComponent == null || Mathf.Approximately(_targetYOffset, targetY)) return;

        _targetYOffset = targetY;
        _lookTween?.Kill();

        _lookTween = DOTween.To(() => _composerComponent.TargetOffset.y,
                                x => {
                                    Vector3 offset = _composerComponent.TargetOffset;
                                    offset.y = x;
                                    _composerComponent.TargetOffset = offset;
                                },
                                targetY, _lookDuration)
                            .SetEase(Ease.InOutSine);
    }
}